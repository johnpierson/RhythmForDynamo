## In Depth

`Geometry.LunchboxShortestWalk(CurveNetwork, Lengths, Paths)`

Find the 'Shortest Walk' within a curve network.  This node uses ported open source code by Giulio Piacentino of McNeel and Associates. Made possible thanks to Proving Ground open-sourcing their package, Lunchbox. provingground.org

The inputs are:

- `CurveNetwork` (_list of Curve_) — A list of curve segments defining a network.
- `Lengths` (_list of number_) — A list of lengths for each curve segment. Length does not need to be "actual" if you want to weight the curves.
- `Paths` (_list of Line_) — A list lines defining the start and end of the path.

The outputs are:

- `Shortest Walk` — The shortest walk path.
- `Links` — Resulting links.
- `Direction` — _Not documented yet._
- `Lengths` — _Not documented yet._

Search terms: `lunchbox`, `curves`, `shortest walk`, `sort`, `path`, `distance`.
