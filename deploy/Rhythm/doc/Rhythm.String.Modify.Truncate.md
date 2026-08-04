## In Depth

`Modify.Truncate(str, length, truncationString: "…")`

This will truncate the given string, byt the given length. (Eg."Long text to truncate", with 10, becomes Long text…") Made possible with Humanizer (https://github.com/Humanizr/Humanizer)

The inputs are:

- `str` (_string_) — The string to truncate.
- `length` (_integer_) — The target length of the string.
- `truncationString` (_string_, defaults to `"…"`) — The characters to fill in the string with.

Returns `truncatedString` (_string_) — The truncated string.

Found in the library under **Actions**.
