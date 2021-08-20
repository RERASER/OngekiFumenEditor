﻿using OngekiFumenEditor.Base;
using OngekiFumenEditor.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OngekiFumenEditor.Parser
{
    [Export(typeof(IOngekiFumenParser))]
    public class DefaultOngekiFumenParser : IOngekiFumenParser
    {
        [ImportMany]
        public IEnumerable<ICommandParser> CommandParsers { get; private set; }

        public async Task<OngekiFumen> ParseAsync(Stream stream)
        {
            var reader = new StreamReader(stream);
            var genObjList = new List<(IOngekiObject obj,ICommandParser parser)>();
            var fumen = new OngekiFumen();

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();

                if (CommandParsers.FirstOrDefault(x=> line.StartsWith(x.CommandLineHeader,StringComparison.OrdinalIgnoreCase)) is ICommandParser parser)
                {
                    var obj = parser.Parse(line,fumen);
                    if (obj!=null)
                    {
                        genObjList.Add((obj,parser));
                        fumen.AddObject(obj);
                    }
                    else
                    {
                        Log.LogWarn($"Can't parse line into object:\"{line}\"");
                    }
                }
            }

            foreach (var pair in genObjList)
            {
                pair.parser.AfterParse(pair.obj, fumen);
            }

            return fumen;
        }
    }
}
