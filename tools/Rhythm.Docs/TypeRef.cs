using System.Collections.Immutable;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;

namespace Rhythm.Docs;

/// <summary>
/// A type as it appears in a signature, kept as a tree rather than a string.
///
/// Two very different names have to come out of the same signature: the documentation ID the
/// compiler wrote into RhythmRevit.xml (<c>System.Collections.Generic.List{Revit.Elements.Element}</c>)
/// and the name a reader should see on a help page ("list of Element"). Formatting one and then
/// trying to parse the other back out of it is how subtly wrong lookups happen, so the shape is
/// kept and both are rendered from it.
/// </summary>
internal sealed class TypeRef
{
    internal required string Name { get; init; }

    internal IReadOnlyList<TypeRef> Arguments { get; init; } = Array.Empty<TypeRef>();

    /// <summary>Set for arrays, by-ref parameters and pointers; <see cref="Name"/> is unused then.</summary>
    internal TypeRef? Element { get; init; }

    internal Decoration Decorated { get; init; }

    internal enum Decoration
    {
        None,
        Array,
        ByRef,
        Pointer,
    }

    /// <summary>
    /// The member-name form from the C# specification's annex on documentation comments: fully
    /// qualified, generic arguments in braces rather than angle brackets and with no arity, arrays
    /// suffixed <c>[]</c> and by-ref parameters <c>@</c>.
    ///
    /// This is what the <c>name</c> attribute in RhythmRevit.xml is built from, so it has to match
    /// character for character or the summary written above a node is never found.
    /// </summary>
    internal string DocId()
    {
        switch (Decorated)
        {
            case Decoration.Array:
                return Element!.DocId() + "[]";
            case Decoration.ByRef:
                return Element!.DocId() + "@";
            case Decoration.Pointer:
                return Element!.DocId() + "*";
        }

        return Arguments.Count == 0
            ? Name
            : Name + "{" + string.Join(",", Arguments.Select(argument => argument.DocId())) + "}";
    }

    /// <summary>The name as a sentence would say it: "list of Element", "boolean", "Revit Document".</summary>
    internal string Display()
    {
        switch (Decorated)
        {
            case Decoration.Array:
                return "list of " + Element!.Display();
            case Decoration.ByRef:
            case Decoration.Pointer:
                return Element!.Display();
        }

        if (Arguments.Count > 0)
        {
            string container = Short(Name);

            // A list of somethings reads better than List<Element>, and Dynamo draws it as a list
            // port either way. Dictionary is left alone because both halves matter to the reader.
            return container is "List" or "IEnumerable" or "IList" or "IReadOnlyList" or "ICollection"
                ? "list of " + Arguments[0].Display()
                : container + " of " + string.Join(" and ", Arguments.Select(argument => argument.Display()));
        }

        return Name switch
        {
            "System.String" => "string",
            "System.Boolean" => "boolean",
            "System.Int32" or "System.Int64" => "integer",
            "System.Double" or "System.Single" => "number",
            "System.Object" => "object",
            "System.Void" => "nothing",
            _ => Short(Name),
        };
    }

    private static string Short(string name)
    {
        int dot = name.LastIndexOf('.');
        return dot < 0 ? name : name[(dot + 1)..];
    }

    internal static TypeRef Of(string name) => new() { Name = name };
}

/// <summary>
/// Turns the bytes of a method signature into <see cref="TypeRef"/>s.
///
/// Every member here is required by the interface; most of them describe constructs Rhythm's node
/// signatures never contain, and exist so that a signature which does contain one produces a
/// readable name rather than an exception halfway through a build.
/// </summary>
internal sealed class SignatureTypes : ISignatureTypeProvider<TypeRef, object?>
{
    private readonly MetadataReader _reader;

    internal SignatureTypes(MetadataReader reader) => _reader = reader;

    public TypeRef GetArrayType(TypeRef elementType, ArrayShape shape)
        => new() { Name = string.Empty, Element = elementType, Decorated = TypeRef.Decoration.Array };

    public TypeRef GetSZArrayType(TypeRef elementType)
        => new() { Name = string.Empty, Element = elementType, Decorated = TypeRef.Decoration.Array };

    public TypeRef GetByReferenceType(TypeRef elementType)
        => new() { Name = string.Empty, Element = elementType, Decorated = TypeRef.Decoration.ByRef };

    public TypeRef GetPointerType(TypeRef elementType)
        => new() { Name = string.Empty, Element = elementType, Decorated = TypeRef.Decoration.Pointer };

    public TypeRef GetGenericInstantiation(TypeRef genericType, ImmutableArray<TypeRef> typeArguments)
        => new() { Name = genericType.Name, Arguments = typeArguments };

    public TypeRef GetGenericMethodParameter(object? genericContext, int index) => TypeRef.Of("``" + index);

    public TypeRef GetGenericTypeParameter(object? genericContext, int index) => TypeRef.Of("`" + index);

    public TypeRef GetModifiedType(TypeRef modifier, TypeRef unmodifiedType, bool isRequired) => unmodifiedType;

    public TypeRef GetPinnedType(TypeRef elementType) => elementType;

    public TypeRef GetFunctionPointerType(MethodSignature<TypeRef> signature) => TypeRef.Of("System.IntPtr");

    public TypeRef GetPrimitiveType(PrimitiveTypeCode typeCode) => TypeRef.Of(typeCode switch
    {
        PrimitiveTypeCode.Boolean => "System.Boolean",
        PrimitiveTypeCode.Byte => "System.Byte",
        PrimitiveTypeCode.Char => "System.Char",
        PrimitiveTypeCode.Double => "System.Double",
        PrimitiveTypeCode.Int16 => "System.Int16",
        PrimitiveTypeCode.Int32 => "System.Int32",
        PrimitiveTypeCode.Int64 => "System.Int64",
        PrimitiveTypeCode.IntPtr => "System.IntPtr",
        PrimitiveTypeCode.Object => "System.Object",
        PrimitiveTypeCode.SByte => "System.SByte",
        PrimitiveTypeCode.Single => "System.Single",
        PrimitiveTypeCode.String => "System.String",
        PrimitiveTypeCode.TypedReference => "System.TypedReference",
        PrimitiveTypeCode.UInt16 => "System.UInt16",
        PrimitiveTypeCode.UInt32 => "System.UInt32",
        PrimitiveTypeCode.UInt64 => "System.UInt64",
        PrimitiveTypeCode.UIntPtr => "System.UIntPtr",
        PrimitiveTypeCode.Void => "System.Void",
        _ => "System.Object",
    });

    public TypeRef GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte rawTypeKind)
        => TypeRef.Of(Names.Of(reader, reader.GetTypeDefinition(handle)));

    public TypeRef GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind)
        => TypeRef.Of(Names.Of(reader, reader.GetTypeReference(handle)));

    public TypeRef GetTypeFromSpecification(
        MetadataReader reader, object? genericContext, TypeSpecificationHandle handle, byte rawTypeKind)
        => reader.GetTypeSpecification(handle).DecodeSignature(this, genericContext);
}

/// <summary>Full names for the metadata handles, with the arity backtick and <c>+</c> nesting removed.</summary>
internal static class Names
{
    internal static string Of(MetadataReader reader, TypeDefinition type)
    {
        string name = Strip(reader.GetString(type.Name));

        // A nested type is written with a dot in a documentation ID, unlike reflection's plus sign.
        if (type.IsNested)
        {
            return Of(reader, reader.GetTypeDefinition(type.GetDeclaringType())) + "." + name;
        }

        string space = reader.GetString(type.Namespace);
        return string.IsNullOrEmpty(space) ? name : space + "." + name;
    }

    internal static string Of(MetadataReader reader, TypeReference type)
    {
        string name = Strip(reader.GetString(type.Name));

        if (type.ResolutionScope.Kind == HandleKind.TypeReference)
        {
            TypeReference declaring = reader.GetTypeReference((TypeReferenceHandle)type.ResolutionScope);
            return Of(reader, declaring) + "." + name;
        }

        string space = reader.GetString(type.Namespace);
        return string.IsNullOrEmpty(space) ? name : space + "." + name;
    }

    /// <summary><c>List`1</c> is <c>List</c> once the arguments are written out in braces.</summary>
    private static string Strip(string name)
    {
        int tick = name.IndexOf('`');
        return tick < 0 ? name : name[..tick];
    }
}
