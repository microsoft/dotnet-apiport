### Recommended Action
Use File.Open to get a Stream then pass it to StreamWriter constructor.

### Affected APIs
* `M:System.IO.StreamWriter.#ctor(System.String)`
* `M:System.IO.StreamWriter.#ctor(System.String,System.Boolean)`
* `M:System.IO.StreamWriter.#ctor(System.String,System.Boolean,System.Text.Encoding)`
* `M:System.IO.StreamWriter.#ctor(System.String,System.Boolean,System.Text.Encoding,System.Int32)`
