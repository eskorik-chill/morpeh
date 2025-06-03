using System;
using System.Collections;
using System.IO;
using System.Reflection;

namespace Scellecs.Morpeh
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
            if (entity.IsDisposed())
            {
                s.WriteLine($"==== <entity: disposed>");
                return;
            }
            
            var arch = entity.GetWorld().entities[entity.Id].currentArchetype;
            if(arch == null) {
                return;
            }
            s.WriteLine($"==== <entity: {entity.Id}>");
            
            foreach (var typeId in arch.components) {
                var def  = ExtendedComponentId.Get(typeId);
                var data = def.entityGetComponentBoxed.Invoke(entity);
                WriteComponent(data, def.type, s);
            }
            s.Write("\n");
        }

        public static void WriteComponent<T>(T data, TextWriter s)
        {
            WriteComponent(data, typeof(T), s);
        }
        
        private static void WriteComponent(object data, Type type, TextWriter s)
        {
            s.WriteLine("[" + type.Name + "]");
            foreach (var field in type.GetFields())
            {
                if(!field.IsPublic || field.MemberType != MemberTypes.Field) continue;
                
                s.Write("  ");
                s.Write(field.Name);
                s.Write(": ");
                WriteFieldValue(field.GetValue(data), s);
            }
        }

        private static void WriteFieldValue(object data, TextWriter s)
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
                case Entity entity:
                    s.Write(entity.IsDisposed() ? "<entity: disposed>" : $"<entity: {entity.Id}>");
                    return;
                default: s.Write(data?.ToString() ?? "null"); return;
            }
        }
    }
}