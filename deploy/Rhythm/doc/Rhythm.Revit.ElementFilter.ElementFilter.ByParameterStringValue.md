## In Depth

`ElementFilter.ByParameterStringValue(elements, parameterName, value, filterMethod)`

Provides element filtering options by parameter string value. For the filter method, we are using something called "LevenshteinDistance". This was introduced to me here, http://dynamobim.org/fuzzy-string-matching/.

The inputs are:

- `elements` (_list of Element_) — The elements to filter.
- `parameterName` (_string_) — The parameter name to filter against..
- `value` (_string_) — The value to filter by.
- `filterMethod` (_string_) — The method to filter by. This includes Contains, DoesNotContain, StartsWith, DoesNotStartWith, EndsWith, DoesNotEndWith, Equals, DoesNotEqual

Returns `elements` (_list of Element_) — The filtered elements.

Found in the library under **Actions**.

Search terms: `ElementFilter`, `Filter.ByName`.
