using System.Collections.Immutable;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace Rhythm.Docs;

internal enum NodeKind
{
    /// <summary>A public static method Dynamo imports as a zero-touch node.</summary>
    ZeroTouch,

    /// <summary>A class deriving from NodeModel — the dropdowns and selection nodes in RhythmUI.</summary>
    NodeModel,
}

internal sealed class NodeParameter
{
    internal required string Name { get; init; }

    internal required TypeRef Type { get; init; }

    /// <summary>The value Dynamo pre-fills the port with, from <c>[DefaultArgument]</c> or an optional parameter.</summary>
    internal string? Default { get; init; }
}

internal sealed class Node
{
    internal required string Assembly { get; init; }

    internal required NodeKind Kind { get; init; }

    /// <summary>Namespace and class, e.g. <c>Rhythm.Revit.Elements.Elements</c>.</summary>
    internal required string DeclaringType { get; init; }

    /// <summary>The method name for a zero-touch node; empty for a NodeModel node, which is the class itself.</summary>
    internal string Member { get; init; } = string.Empty;

    internal IReadOnlyList<NodeParameter> Parameters { get; init; } = Array.Empty<NodeParameter>();

    internal TypeRef Returns { get; init; } = TypeRef.Of("System.Void");

    /// <summary>The output names from <c>[MultiReturn]</c>, empty for a node with a single output.</summary>
    internal IReadOnlyList<string> ReturnKeys { get; init; } = Array.Empty<string>();

    /// <summary>The name shown on a NodeModel node in the library, from <c>[NodeName]</c>.</summary>
    internal string? DisplayName { get; init; }

    /// <summary>The one-line help on a NodeModel node, from <c>[NodeDescription]</c>.</summary>
    internal string? Description { get; init; }

    /// <summary>Where the node sits in the library, from <c>[NodeCategory]</c>.</summary>
    internal string? Category { get; init; }

    /// <summary>
    /// The name Dynamo looks the help file up by — <c>DynamoViewModel.GetMinimumQualifiedName</c>.
    /// A zero-touch node is its qualified method name; a NodeModel node is its type's full name.
    /// </summary>
    internal string QualifiedName
        => Kind == NodeKind.ZeroTouch ? DeclaringType + "." + Member : DeclaringType;

    /// <summary>The name as it reads on a node in a graph.</summary>
    internal string ShortName
    {
        get
        {
            if (Kind == NodeKind.NodeModel)
            {
                return DisplayName ?? DeclaringType[(DeclaringType.LastIndexOf('.') + 1)..];
            }

            return DeclaringType[(DeclaringType.LastIndexOf('.') + 1)..] + "." + Member;
        }
    }

    /// <summary>
    /// The <c>member name</c> attribute the compiler wrote into the XML documentation file, so the
    /// summary above the source can be found again.
    /// </summary>
    internal string DocId
    {
        get
        {
            if (Kind == NodeKind.NodeModel)
            {
                return "T:" + DeclaringType;
            }

            string name = "M:" + DeclaringType + "." + Member;

            return Parameters.Count == 0
                ? name
                : name + "(" + string.Join(",", Parameters.Select(p => p.Type.DocId())) + ")";
        }
    }
}

/// <summary>
/// Reads the node list straight out of an assembly's metadata tables.
///
/// Deliberately never loads the assembly. RhythmRevit.dll is bound to a specific Revit API and
/// RhythmUI.dll to a specific Dynamo, so <c>Assembly.LoadFrom</c> would need a matching Revit
/// installed just to list method names; the metadata tables carry every name this needs and
/// resolve nothing.
/// </summary>
internal static class NodeReader
{
    private const string DynamoRuntime = "Autodesk.DesignScript.Runtime";

    internal static IReadOnlyList<Node> Read(string assemblyPath)
    {
        using FileStream file = File.OpenRead(assemblyPath);
        using PEReader pe = new(file);

        MetadataReader reader = pe.GetMetadataReader();
        SignatureTypes signatures = new(reader);
        string assembly = Path.GetFileNameWithoutExtension(assemblyPath);

        List<Node> nodes = new();

        foreach (TypeDefinitionHandle handle in reader.TypeDefinitions)
        {
            TypeDefinition type = reader.GetTypeDefinition(handle);

            if (type.IsNested || !IsPublic(type) || !IsRhythm(reader, type))
            {
                continue;
            }

            Attributes typeAttributes = Attributes.On(reader, type.GetCustomAttributes());

            if (typeAttributes.Hidden)
            {
                continue;
            }

            // A class carrying [NodeName] is a NodeModel node: Dynamo places the class itself, and
            // there is no method to document. Nothing else in Rhythm uses that attribute, which is
            // what makes it a safe test without resolving DSRevitNodesUI to walk the base chain.
            if (typeAttributes.NodeName is { } nodeName)
            {
                if (type.Attributes.HasFlag(TypeAttributes.Abstract))
                {
                    continue;
                }

                nodes.Add(new Node
                {
                    Assembly = assembly,
                    Kind = NodeKind.NodeModel,
                    DeclaringType = Names.Of(reader, type),
                    DisplayName = nodeName,
                    Description = typeAttributes.NodeDescription,
                    Category = typeAttributes.NodeCategory,
                });

                continue;
            }

            foreach (MethodDefinitionHandle methodHandle in type.GetMethods())
            {
                if (ReadMethod(reader, signatures, assembly, type, methodHandle) is { } node)
                {
                    nodes.Add(node);
                }
            }
        }

        return nodes;
    }

    private static Node? ReadMethod(
        MetadataReader reader,
        SignatureTypes signatures,
        string assembly,
        TypeDefinition type,
        MethodDefinitionHandle handle)
    {
        MethodDefinition method = reader.GetMethodDefinition(handle);
        MethodAttributes attributes = method.Attributes;

        // Zero-touch imports public static methods. Property accessors, operators and the static
        // constructor are all public statics too, and none of them is a node.
        if (!attributes.HasFlag(MethodAttributes.Public) ||
            !attributes.HasFlag(MethodAttributes.Static) ||
            attributes.HasFlag(MethodAttributes.SpecialName) ||
            attributes.HasFlag(MethodAttributes.RTSpecialName))
        {
            return null;
        }

        // An open generic method has no concrete signature for Dynamo to build ports from, so the
        // zero-touch importer passes over it. RhythmUI.Utilities.SelFilter has several, and two of
        // them differ only in a type argument — documenting those would produce two pages fighting
        // over one file name for a pair of nodes that do not exist.
        if (method.GetGenericParameters().Count > 0)
        {
            return null;
        }

        Attributes methodAttributes = Attributes.On(reader, method.GetCustomAttributes());

        if (methodAttributes.Hidden)
        {
            return null;
        }

        MethodSignature<TypeRef> signature = method.DecodeSignature(signatures, genericContext: null);
        List<NodeParameter> parameters = new();

        foreach (ParameterHandle parameterHandle in method.GetParameters())
        {
            Parameter parameter = reader.GetParameter(parameterHandle);

            // Sequence number 0 is the return value, which carries attributes but is not an input.
            if (parameter.SequenceNumber == 0)
            {
                continue;
            }

            int index = parameter.SequenceNumber - 1;

            if (index >= signature.ParameterTypes.Length)
            {
                continue;
            }

            parameters.Add(new NodeParameter
            {
                Name = reader.GetString(parameter.Name),
                Type = signature.ParameterTypes[index],
                Default = DefaultOf(reader, parameter),
            });
        }

        return new Node
        {
            Assembly = assembly,
            Kind = NodeKind.ZeroTouch,
            DeclaringType = Names.Of(reader, type),
            Member = reader.GetString(method.Name),
            Parameters = parameters,
            Returns = signature.ReturnType,
            ReturnKeys = methodAttributes.ReturnKeys,
            Category = methodAttributes.NodeCategory,
        };
    }

    /// <summary>
    /// The value Dynamo shows on an unconnected port: the expression from
    /// <c>[DefaultArgument]</c> if there is one, otherwise the optional parameter's own default.
    /// </summary>
    private static string? DefaultOf(MetadataReader reader, Parameter parameter)
    {
        Attributes attributes = Attributes.On(reader, parameter.GetCustomAttributes());

        if (attributes.DefaultArgument is { } expression)
        {
            return expression;
        }

        ConstantHandle constant = parameter.GetDefaultValue();

        if (constant.IsNil)
        {
            return null;
        }

        Constant value = reader.GetConstant(constant);
        BlobReader blob = reader.GetBlobReader(value.Value);

        return value.TypeCode switch
        {
            ConstantTypeCode.Boolean => blob.ReadBoolean() ? "true" : "false",
            ConstantTypeCode.String => "\"" + blob.ReadUTF16(blob.RemainingBytes) + "\"",
            ConstantTypeCode.Int32 => blob.ReadInt32().ToString(),
            ConstantTypeCode.Int64 => blob.ReadInt64().ToString(),
            ConstantTypeCode.Double => blob.ReadDouble().ToString("0.####"),
            ConstantTypeCode.Single => blob.ReadSingle().ToString("0.####"),
            ConstantTypeCode.NullReference => "null",
            _ => null,
        };
    }

    private static bool IsPublic(TypeDefinition type)
        => (type.Attributes & TypeAttributes.VisibilityMask) == TypeAttributes.Public;

    /// <summary>
    /// Rhythm's own types only. The assemblies also carry helpers and a few vendored types, and
    /// documenting those would offer help for nodes nobody can place.
    /// </summary>
    private static bool IsRhythm(MetadataReader reader, TypeDefinition type)
    {
        string space = reader.GetString(type.Namespace);
        return space == "Rhythm" || space.StartsWith("Rhythm.", StringComparison.Ordinal) ||
               space == "RhythmUI" || space.StartsWith("RhythmUI.", StringComparison.Ordinal);
    }

    /// <summary>The handful of attributes that change what a node is or how it is documented.</summary>
    private sealed class Attributes
    {
        internal bool Hidden { get; private set; }

        internal string? NodeName { get; private set; }

        internal string? NodeDescription { get; private set; }

        internal string? NodeCategory { get; private set; }

        internal string? DefaultArgument { get; private set; }

        internal IReadOnlyList<string> ReturnKeys { get; private set; } = Array.Empty<string>();

        internal static Attributes On(MetadataReader reader, CustomAttributeHandleCollection handles)
        {
            Attributes found = new();
            AttributeValues values = new(reader);

            foreach (CustomAttributeHandle handle in handles)
            {
                CustomAttribute attribute = reader.GetCustomAttribute(handle);

                if (NameOf(reader, attribute) is not { } name)
                {
                    continue;
                }

                // Attribute blobs are only decoded for the few that matter. Decoding every one
                // would mean handling enum arguments whose underlying type lives in an assembly
                // that is deliberately not being resolved.
                switch (name)
                {
                    case "IsVisibleInDynamoLibraryAttribute":
                        found.Hidden = Arguments(attribute, values) is [{ Value: false }];
                        break;

                    case "NodeNameAttribute":
                        found.NodeName = FirstString(attribute, values);
                        break;

                    case "NodeDescriptionAttribute":
                        found.NodeDescription = FirstString(attribute, values);
                        break;

                    case "NodeCategoryAttribute":
                        found.NodeCategory = FirstString(attribute, values);
                        break;

                    case "DefaultArgumentAttribute":
                        found.DefaultArgument = FirstString(attribute, values);
                        break;

                    case "MultiReturnAttribute":
                        found.ReturnKeys = ReturnKeysOf(attribute, values);
                        break;
                }
            }

            return found;
        }

        private static ImmutableArray<CustomAttributeTypedArgument<TypeRef>>? Arguments(
            CustomAttribute attribute, AttributeValues values)
        {
            try
            {
                return attribute.DecodeValue(values).FixedArguments;
            }
            catch (Exception)
            {
                // An attribute whose blob cannot be decoded is one whose argument type lives in an
                // assembly that is not here. Treating it as absent is right: none of these change
                // whether the node exists, only how its page reads.
                return null;
            }
        }

        private static string? FirstString(CustomAttribute attribute, AttributeValues values)
            => Arguments(attribute, values) is [{ Value: string text }, ..] ? text : null;

        /// <summary><c>[MultiReturn]</c> takes a <c>params string[]</c>, so the blob holds one array.</summary>
        private static IReadOnlyList<string> ReturnKeysOf(CustomAttribute attribute, AttributeValues values)
        {
            if (Arguments(attribute, values) is not
                [{ Value: ImmutableArray<CustomAttributeTypedArgument<TypeRef>> keys }])
            {
                return Array.Empty<string>();
            }

            return keys.Select(key => key.Value as string ?? string.Empty)
                .Where(key => key.Length > 0)
                .ToArray();
        }

        private static string? NameOf(MetadataReader reader, CustomAttribute attribute)
        {
            switch (attribute.Constructor.Kind)
            {
                case HandleKind.MemberReference:
                    MemberReference member = reader.GetMemberReference((MemberReferenceHandle)attribute.Constructor);

                    return member.Parent.Kind == HandleKind.TypeReference
                        ? reader.GetString(reader.GetTypeReference((TypeReferenceHandle)member.Parent).Name)
                        : null;

                case HandleKind.MethodDefinition:
                    MethodDefinition constructor =
                        reader.GetMethodDefinition((MethodDefinitionHandle)attribute.Constructor);

                    return reader.GetString(reader.GetTypeDefinition(constructor.GetDeclaringType()).Name);

                default:
                    return null;
            }
        }
    }

    /// <summary>Decodes attribute arguments. Only strings, booleans and string arrays are ever asked for.</summary>
    private sealed class AttributeValues : ICustomAttributeTypeProvider<TypeRef>
    {
        private readonly SignatureTypes _types;

        internal AttributeValues(MetadataReader reader) => _types = new SignatureTypes(reader);

        public TypeRef GetPrimitiveType(PrimitiveTypeCode typeCode) => _types.GetPrimitiveType(typeCode);

        public TypeRef GetSZArrayType(TypeRef elementType) => _types.GetSZArrayType(elementType);

        public TypeRef GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte rawTypeKind)
            => _types.GetTypeFromDefinition(reader, handle, rawTypeKind);

        public TypeRef GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind)
            => _types.GetTypeFromReference(reader, handle, rawTypeKind);

        public TypeRef GetSystemType() => TypeRef.Of("System.Type");

        public bool IsSystemType(TypeRef type) => type.Name == "System.Type";

        public TypeRef GetTypeFromSerializedName(string name) => TypeRef.Of(name.Split(',')[0]);

        /// <summary>
        /// Reached only by an attribute with an enum argument. Resolving the underlying type would
        /// mean loading the assembly that declares it, which is the one thing this reader does not
        /// do; the caller treats the failure as an absent attribute.
        /// </summary>
        public PrimitiveTypeCode GetUnderlyingEnumType(TypeRef type)
            => throw new NotSupportedException($"Enum argument of type {type.Name} in an attribute blob.");
    }
}
