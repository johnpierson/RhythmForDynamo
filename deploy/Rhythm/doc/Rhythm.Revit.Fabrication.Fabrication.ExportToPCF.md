## In Depth

`Fabrication.ExportToPCF(fileName, elements)`

This node exports a list of fabrication elements to a PCF (Piping Component File) format. It filters the provided elements to include only FabricationPart instances and then uses Revit's FabricationUtils to perform the export operation.

The inputs are:

- `fileName` (_string_) — The full path of the PCF file to export to.
- `elements` (_list of Element_) — A list of Revit elements to be exported. Only FabricationPart elements are considered.

Returns `resultMessage` (_string_) — A message indicating the success or failure of the export operation.
