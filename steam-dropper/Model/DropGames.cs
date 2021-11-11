using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace steam_dropper.Model
{
    public class DropGameConfig
    {
        public uint AppId;
        public ulong DefId;
    }

    public class DropGameList : List<DropGameConfig>
    {

    }
}
