# Introduction
These are recommended actions to take for APIs that are not supported on a .NET platform. The [.NET Portability Analyzer](docs/HowTo/Introduction.md#usage) consumes these as they are updated, so the latest recommendations are used.

## Adding Recommendations
1.  Copy the template: [! Template.md](!%20Template.md)
2.  Fill in a Twitter-sized Recommended Action
3.  Fill in "Affected APIs"
    1. Find the highest level that your recommended action affects and use that [docId](#document-comment-identifiers-docid)
    2. Find [docIds](#document-comment-identifiers-docid) using:
        * http://dotnetstatus.azurewebsites.net or 
        * Using the console tool and running: `ApiPort.exe docidsearch`
4.  Place the file in the following folder structure: __namespace__ => __recommended_action.md__
5.  Create a pull request!

### Example
I have a recommended change for the class `System.Threading.Thread`. My change could look like this:

__Full Path:__ `docs/RecommendedChanges/System.Threading/Use Task library.md`

__Content__:
```markdown
### Recommended Action
Use Task library instead.
### Affected APIs
* `T:System.Threading.Thread`
```

## Updating Recommendations
1. Find the `namespace` of the recommendation you want to update
2. Locate the folder with that `namespace` under `docs/RecommendedChanges`
3. Find the .md file with the recommendation
4. Update the contents
5. Create a pull request!

## Note
The .NET Portability Service is aware of the hierarchy of APIs. First, it looks for a recommended change containing that exact docId, then it searches the ancestors of that docId until it reaches the first recommended change or the namespace level.

## Document Comment Identifiers (docId)
Each API is represented as an ID string that is constructed using the guidelines specified in [Processing the XML File](https://msdn.microsoft.com/en-us/library/fsbx0t7x.aspx). Microsoft.Fx.Portability.MetadataReader decodes docIds using this specification (ie. [DependencyFinderEngineHelper.cs](../../src/Microsoft.Fx.Portability.MetadataReader/DependencyFinderEngineHelper.cs#L160))