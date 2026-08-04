## In Depth

`Dimensions.ValueString(dimension)`

This node will return the value (string) of the dimension. If the dimension is a multi-segment dimension, this will find all of the above values. This method returns what the dimension would be in it's non-rounded form. If you want the actual displayed string use Dimension.DisplayValueString in Rhythm.

The inputs are:

- `dimension` (_Dimension_) — The dimension to obtain value from.

Returns `valueString` (_list of string_) — The dimension value as a string.

Found in the library under **Query**.

Search terms: `dimension.TextPosition`.
