## In Depth

`Element.AnimateNumericParameter(element, parameterName, startValue, endValue, iterations, directoryPath)`

Animate a numeric parameter of an element. This will export images of the parameter, then revert the element back to where it was. Also adds text to comments to prevent infinite loops.Clear this comment for subsequent runs. Inspired by the Bad Monkeys Team.

The inputs are:

- `element` (_list of Element_) — The element to set parameter to.
- `parameterName` (_string_) — The parameter name.
- `startValue` (_number_) — The value to set.
- `endValue` (_number_) — The value to set.
- `iterations` (_integer_) — The number of images.
- `directoryPath` (_string_) — Where to save the images.

Returns `element` (_object_) — The element.

Search terms: `rhythm`.
