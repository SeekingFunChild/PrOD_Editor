using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PrOD_Editor
{
    class PrOD
    {
        public class Header
        {
            public static uint size=0x20;

            public string magic;
            public uint constNum1;
            public uint constNum2;
            public uint lstUSOffset; //US --Uniform Scale
            public uint fileSize;
            public uint meshCount;
            public uint STOffset;
            public uint NullPadding = 0x00000000;

            public Header()
            {
                magic = "PrOD";
                constNum1 = 0x01000000;
                constNum2 = 0x00000001;
            }

            public Header(byte[] bytes)
            {
                magic = EndianBytesOperator.readString(bytes, 0, 4);
                if(magic!="PrOD")
                {
                    throw new InvalidDataException();
                }

                constNum1 = EndianBytesOperator.readUInt(bytes, 0x04);
                if(constNum1!=0x01000000)
                {
                    throw new InvalidDataException();
                }

                constNum2 = EndianBytesOperator.readUInt(bytes, 0x08);
                if(constNum2!=0x00000001)
                {
                    throw new InvalidDataException();
                }

                lstUSOffset= EndianBytesOperator.readUInt(bytes, 0x0C);
                fileSize= EndianBytesOperator.readUInt(bytes, 0x10);
                meshCount= EndianBytesOperator.readUInt(bytes, 0x14);
                STOffset = EndianBytesOperator.readUInt(bytes, 0x18);

                NullPadding= EndianBytesOperator.readUInt(bytes, 0x1C);
                if(NullPadding!=0x00000000)
                {
                    throw new InvalidDataException();
                }
            }

            public void writeToBytes(byte[] bytes)
            {
                EndianBytesOperator.writeString(bytes, 0x0, magic,4);
                EndianBytesOperator.writeUInt(bytes, 0x04, constNum1);
                EndianBytesOperator.writeUInt(bytes, 0x08, constNum2);
                EndianBytesOperator.writeUInt(bytes, 0x0C, lstUSOffset);
                EndianBytesOperator.writeUInt(bytes, 0x10, fileSize);
                EndianBytesOperator.writeUInt(bytes, 0x14, meshCount);
                EndianBytesOperator.writeUInt(bytes, 0x18, STOffset);
                EndianBytesOperator.writeUInt(bytes, 0x1C, NullPadding);
            }
        }

        public class Mesh
        {
            public class MeshInstance
            {
                public static uint size=0x20;

                public float[] position;
                public float[] rotation;
                public float uniformScale;
                public uint NullPadding=0x0;

                public MeshInstance(byte[] bytes,uint offset)
                {
                    position = new float[3];
                    position[0] = EndianBytesOperator.readFloat(bytes, (int)offset + 0x0);
                    position[1] = EndianBytesOperator.readFloat(bytes, (int)offset + 0x04);
                    position[2] = EndianBytesOperator.readFloat(bytes, (int)offset + 0x08);

                    rotation = new float[3];
                    rotation[0] = EndianBytesOperator.readFloat(bytes, (int)offset + 0x0C);
                    rotation[1] = EndianBytesOperator.readFloat(bytes, (int)offset + 0x10);
                    rotation[2] = EndianBytesOperator.readFloat(bytes, (int)offset + 0x14);

                    uniformScale = EndianBytesOperator.readFloat(bytes, (int)offset + 0x18);
                }

                public MeshInstance(float[] pos,float[] rot,float ufscale)
                {
                    position = pos;
                    rotation = rot;
                    uniformScale = ufscale;
                }

                public void writeToBytes(byte[] bytes,uint offset)
                {
                    EndianBytesOperator.writeFloat(bytes, (int)offset + 0x00,position[0]);
                    EndianBytesOperator.writeFloat(bytes, (int)offset + 0x04,position[1]);
                    EndianBytesOperator.writeFloat(bytes, (int)offset + 0x08,position[2]);

                    EndianBytesOperator.writeFloat(bytes, (int)offset + 0x0C,rotation[0]);
                    EndianBytesOperator.writeFloat(bytes, (int)offset + 0x10,rotation[1]);
                    EndianBytesOperator.writeFloat(bytes, (int)offset + 0x14,rotation[2]);

                    EndianBytesOperator.writeFloat(bytes, (int)offset + 0x18,uniformScale);

                    EndianBytesOperator.writeUInt(bytes, (int)offset + 0x1C, NullPadding);
                }
            }

            private uint m_size;
            public uint size
            {
                set
                {
                    m_size = value;
                }
                get
                {
                    return instancesSize + 0x10;
                }
            }

            public string name;

            private uint m_instancesSize;
            public uint instancesSize{
                set
                {
                    m_instancesSize = value;
                }
                get {
                    return instancesCount * 0x20;
                }
            }

            public uint instancesCount;
            public uint nameOffset;
            public uint NullPadding=0x0;

            public List<MeshInstance> instances;

            public Mesh(byte[] bytes,uint offset,List<string> names,uint stOffset)
            {
                instancesSize= EndianBytesOperator.readUInt(bytes,(int)offset+0x0);
                size = 0x10 + instancesSize;

                instancesCount= EndianBytesOperator.readUInt(bytes, (int)offset + 0x04);
                nameOffset= EndianBytesOperator.readUInt(bytes, (int)offset + 0x08);
                name = EndianBytesOperator.readString(bytes,(int)(stOffset+nameOffset));
                names.Add(name);

                instances = new List<MeshInstance>();
                for(int i=0;i<instancesCount;i++)
                {
                    instances.Add(new MeshInstance(bytes,(uint)(offset+0x10+MeshInstance.size*i)));
                }
            }

            public Mesh()
            {
                instances = new List<MeshInstance>();
            }

            public void writeToBytes(byte[] bytes,uint offset)
            {
                EndianBytesOperator.writeUInt(bytes, (int)offset + 0x00, instancesSize);
                EndianBytesOperator.writeUInt(bytes, (int)offset + 0x04, instancesCount);
                EndianBytesOperator.writeUInt(bytes, (int)offset + 0x08, nameOffset);
                EndianBytesOperator.writeUInt(bytes, (int)offset + 0x0C, NullPadding);

                uint t_offset = offset + 0x10;
                foreach(MeshInstance meshIns in instances)
                {
                    meshIns.writeToBytes(bytes, t_offset);
                    t_offset += MeshInstance.size;
                }
            }
        }

        private byte[] m_bytes;
        public Header m_header;
        public List<Mesh> m_meshes;
        public List<string> names;

        private uint m_meshesSize
        {
            get
            {
                uint size = 0;
                foreach(Mesh mesh in m_meshes)
                {
                    size += mesh.size;
                }
                return size;
            }
        }

        private uint m_namesSize
        {
            get
            {
                uint size = 0;
                foreach(string str in names)
                {
                    size += (uint)str.Length + 2;
                    while(size%4!=0)
                    {
                        size++;
                    }
                }
                return size+0x08;
            }
        }

        public PrOD(string filePath)
        {
            m_bytes = Yaz0.decode(File.ReadAllBytes(filePath));
            Init();
        }

        public PrOD(byte[] bytes)
        {
            m_bytes = bytes;
            Init();
        }

        public PrOD(List<string> t_names,List<Mesh> meshes)
        {
            m_meshes = meshes;
            names = t_names;

            m_header = new Header();
            m_header.meshCount = (uint)meshes.Count;
            m_header.STOffset = Header.size + m_meshesSize;
            m_header.fileSize = Header.size + m_meshesSize + m_namesSize;
            m_header.lstUSOffset = m_header.STOffset - 0x08;
        }

        private void Init()
        {
            m_header = new Header(m_bytes);

            m_meshes = new List<Mesh>();
            names = new List<string>();
            uint offset = Header.size;
            for(int i=0;i<m_header.meshCount;i++)
            {
                Mesh t_mesh = new Mesh(m_bytes, offset,names,m_header.STOffset);
                offset += t_mesh.size;
                m_meshes.Add(t_mesh);            
            }
        }

        public static uint getNameOffset(List<string> names,string name)
        {
            int index = names.IndexOf(name);
            int offset = 0x08;
            if(index<0)
            {
                return 0;
            }
            for(int i=0;i<index;i++)
            {
                int len = names[i].Length+2;
                while(len%4!=0)
                {
                    len++;
                }
                offset += len;
            }

            return (uint)offset;
        }

        public byte[] getBytes()
        {
            if (m_bytes != null)
            {
                return m_bytes;
            }
            m_bytes = new byte[m_header.fileSize];
            m_header.writeToBytes(m_bytes);

            uint offset=Header.size;
            foreach(var mesh in m_meshes)
            {
                mesh.writeToBytes(m_bytes, offset);
                offset += mesh.size;
            }

            offset = m_header.STOffset;
            EndianBytesOperator.writeUInt(m_bytes, (int)offset + 0x00, (uint)names.Count);
            EndianBytesOperator.writeUInt(m_bytes, (int)offset + 0x04, m_namesSize-0x08);
            offset += 0x08;

            foreach (var str in names)
            {
                offset += (uint)EndianBytesOperator.writeString_PrOD(m_bytes, (int)offset, str);
            }

            return m_bytes;
        }
    }
}
