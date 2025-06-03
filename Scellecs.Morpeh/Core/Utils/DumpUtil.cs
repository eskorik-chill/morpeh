using System;
using System.Collections;
using System.IO;

namespace Scellecs.Morpeh.Core.Utils
{
    public static class DumpUtil
    {
        public static string Dump(this World world) {
            var sb = new StringWriter();
            Dump(world, sb);
            return sb.ToString();
        }
        
        public static string Dump(this Entity entity) {
            var sb = new StringWriter();
            Dump(entity, sb);
            return sb.ToString();
        }

        public static void Dump(this World world, TextWriter s) {
            for(var i = 1; i < world.entitiesCount; i++) {
                world.GetEntityAtIndex(i).Dump(s);
            }
        }
        
        public static void Dump(this Entity entity, TextWriter s) {
            s.WriteLine($"==== <entity: {entity.Id}>");
            var arch = entity.GetWorld().entities[entity.Id].currentArchetype;
            foreach (var typeId in arch.components) {
                var def  = ExtendedComponentId.Get(typeId);
                var data = def.entityGetComponentBoxed.Invoke(entity);
                if (def.isMarker) {
                    s.WriteLine("[" + def.type.Name + "]");
                } else {
                    WriteComponent(data, s);
                }
            }
            s.Write("\n");
        }
        
        public static void WriteComponent<T>(T data, TextWriter s)
        {
            var type = typeof(T);
            s.WriteLine("[" + type.Name + "]");
            foreach (var field in type.GetFields())
            {
                s.Write(field.Name);
                s.Write(": ");
                WriteFieldValue(field.GetValue(data), s);
            }
        }

        private static void WriteFieldValue<T>(T data, TextWriter s)
        {
            switch (data)
            {
                case IList list:
                    s.Write("{\n");
                    foreach (var x in list)
                    {
                        WriteFieldValue(x, s);
                        s.Write("\n");
                    }
                    s.Write("}\n");
                    return;
                case Enum:
                    s.Write(Enum.GetName(typeof(T), data));
                    return;
                case Entity entity:
                    s.Write(entity.IsDisposed() ? "<entity: disposed>" : $"<entity: {entity.Id}>");
                    return;
                default: s.Write(data?.ToString() ?? "null"); return;
            }
        }
    }
}