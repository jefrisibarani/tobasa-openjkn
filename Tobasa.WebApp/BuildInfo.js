fs = require('fs');

var buildDate = new Date().toISOString().replace('T', ' ').substr(0, 19) + " UTC";

var content =
`
namespace Tobasa 
{ 
   static class BuildInfo 
   {
      public const string Version = "1.0.0";
      public const string BuildDate = "${buildDate}";
   }
}`;

fs.writeFile('./BuildInfo.cs', content, (err) => {
   if (err) throw err;
});