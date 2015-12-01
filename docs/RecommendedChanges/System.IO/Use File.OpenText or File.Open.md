### Recommended Action
Use File.OpenText, or use File.Open to get a Stream then pass it to StreamReader constructor.

### Affected APIs
* `M:System.IO.StreamReader.#ctor(System.String)`
* `M:System.IO.StreamReader.#ctor(System.String,System.Boolean)`
* `M:System.IO.StreamReader.#ctor(System.String,System.Text.Encoding)`
* `M:System.IO.StreamReader.#ctor(System.String,System.Text.Encoding,System.Boolean)`
* `M:System.IO.StreamReader.#ctor(System.String,System.Text.Encoding,System.Boolean,System.Int32)`
