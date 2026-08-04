## In Depth

`ElementFilter.ByParameterNumericValue(elements, parameterName, value, filterMethod)`

Provides element filtering options by parameter numeric value. For the filter method, we are using something called "LevenshteinDistance". This was introduced to me here, http://dynamobim.org/fuzzy-string-matching/.

The inputs are:

- `elements` (_list of Element_) — The elements to filter.
- `parameterName` (_string_) — The parameter name to filter against..
- `value` (_number_) — The value to filter by.
- `filterMethod` (_string_) — The method to filter by. This includes GreaterThan, GreaterThanOrEqualTo, LessThan, LessThanOrEqualTo, EqualTo, NotEqualTo

Returns `elements` (_list of Element_) — The filtered elements.

Found in the library under **Actions**.

Search terms: `ElementFilter`, `Filter.ByName`.
