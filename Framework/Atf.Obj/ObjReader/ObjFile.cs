//Sony Computer Entertainment Confidential

using System;
using System.Collections.Generic;
using System.IO;

using Sce.Atf.Dom;

namespace Sce.Atf.Obj
{
    /// <summary>
    /// Object file</summary>
    public class ObjFile
    {
        /// <summary>
        /// Read object file</summary>
        /// <param name="strm">Stream to read data into</param>
        /// <param name="resolvedUri">URI representing object file</param>
        public void Read(Stream strm, Uri resolvedUri)
        {
            m_resolvedUri = resolvedUri;

            // Create an instance of StreamReader to read from a file
            Parse(new StreamReader(strm));
        }
        
        /// <summary>
        /// Populates DomNode with data from stream data</summary>
        /// <param name="stream">Stream to read data into</param>
        /// <param name="node">Node to populate</param>
        /// <param name="resolvedUri">URI representing object file</param>
        public static void PopulateDomNode(Stream stream, ref DomNode node, Uri resolvedUri)
        {
            // Parse .obj file
            var obj = new ObjFile();
            obj.Read(stream, resolvedUri);

            if (node == null)
                node = new DomNode(Schema.nodeType.Type);

            // Populate mesh
            var mesh = new DomNode(Schema.meshType.Type);
            mesh.SetAttribute(Schema.meshType.nameAttribute, Path.GetFileName(resolvedUri.LocalPath));

            var vertexArray = new DomNode(Schema.meshType_vertexArray.Type);

            // Populate primitive data
            foreach (Group group in obj.m_groups.Values)
                foreach (FaceSet face in group.FaceSets.Values)
                {
                    var primitive = new DomNode(Schema.vertexArray_primitives.Type);
                    primitive.SetAttribute(Schema.vertexArray_primitives.indicesAttribute, face.Indices.ToArray());
                    primitive.SetAttribute(Schema.vertexArray_primitives.sizesAttribute, face.Sizes.ToArray());
                    primitive.SetAttribute(Schema.vertexArray_primitives.typeAttribute, "POLYGONS");

                    // Populate shader
                    MaterialDef material;
                    obj.m_mtl.Materials.TryGetValue(face.MaterialName, out material);

                    if (material != null)
                    {
                        string texture = null;
                        if (material.TextureName != null)
                            texture = new Uri(resolvedUri, material.TextureName).AbsolutePath;

                        var shader = new DomNode(Schema.shaderType.Type);
                        shader.SetAttribute(Schema.shaderType.nameAttribute, material.Name);
                        shader.SetAttribute(Schema.shaderType.ambientAttribute, material.Ambient);
                        shader.SetAttribute(Schema.shaderType.diffuseAttribute, material.Diffuse);
                        shader.SetAttribute(Schema.shaderType.shininessAttribute, material.Shininess);
                        shader.SetAttribute(Schema.shaderType.specularAttribute, material.Specular);
                        shader.SetAttribute(Schema.shaderType.textureAttribute, texture);

                        primitive.SetChild(Schema.vertexArray_primitives.shaderChild, shader);
                    }

                    // Note: Bindings must be in the order: normal, map1, position
                    DomNode binding;
                    if (face.HasNormals)
                    {
                        binding = new DomNode(Schema.primitives_binding.Type);
                        binding.SetAttribute(Schema.primitives_binding.sourceAttribute, "normal");
                        primitive.GetChildList(Schema.vertexArray_primitives.bindingChild).Add(binding);
                    }

                    if (face.HasTexCoords)
                    {
                        binding = new DomNode(Schema.primitives_binding.Type);
                        binding.SetAttribute(Schema.primitives_binding.sourceAttribute, "map1");
                        primitive.GetChildList(Schema.vertexArray_primitives.bindingChild).Add(binding);
                    }

                    binding = new DomNode(Schema.primitives_binding.Type);
                    binding.SetAttribute(Schema.primitives_binding.sourceAttribute, "position");
                    primitive.GetChildList(Schema.vertexArray_primitives.bindingChild).Add(binding);

                    vertexArray.GetChildList(Schema.meshType_vertexArray.primitivesChild).Add(primitive);
                }

            // Populate array data
            DomNode array;
            if (obj.m_normals.Count > 0)
            {
                array = new DomNode(Schema.vertexArray_array.Type);
                array.SetAttribute(Schema.vertexArray_array.Attribute, obj.m_normals.ToArray());
                array.SetAttribute(Schema.vertexArray_array.countAttribute, obj.m_normals.Count / 3);
                array.SetAttribute(Schema.vertexArray_array.nameAttribute, "normal");
                array.SetAttribute(Schema.vertexArray_array.strideAttribute, 3);

                vertexArray.GetChildList(Schema.meshType_vertexArray.arrayChild).Add(array);
            }

            if (obj.m_texcoords.Count > 0)
            {
                array = new DomNode(Schema.vertexArray_array.Type);
                array.SetAttribute(Schema.vertexArray_array.Attribute, obj.m_texcoords.ToArray());
                array.SetAttribute(Schema.vertexArray_array.countAttribute, obj.m_texcoords.Count / 2);
                array.SetAttribute(Schema.vertexArray_array.nameAttribute, "map1");
                array.SetAttribute(Schema.vertexArray_array.strideAttribute, 2);

                vertexArray.GetChildList(Schema.meshType_vertexArray.arrayChild).Add(array);
            }

            array = new DomNode(Schema.vertexArray_array.Type);
            array.SetAttribute(Schema.vertexArray_array.Attribute, obj.m_positions.ToArray());
            array.SetAttribute(Schema.vertexArray_array.countAttribute, obj.m_positions.Count / 3);
            array.SetAttribute(Schema.vertexArray_array.nameAttribute, "position");
            array.SetAttribute(Schema.vertexArray_array.strideAttribute, 3);

            vertexArray.GetChildList(Schema.meshType_vertexArray.arrayChild).Add(array);

            // Set mesh elements
            mesh.SetChild(Schema.meshType.vertexArrayChild, vertexArray);
            node.SetChild(Schema.nodeType.meshChild, mesh);
        }

        private void Parse(TextReader sr)
        {
            string buf;

            // Read lines from the file until EOF
            while ((buf = sr.ReadLine()) != null)
            {
                buf = buf.Trim();
                if (string.IsNullOrEmpty(buf)) continue;
                if (buf[0] == '#') continue;

                string[] split = buf.Split(s_stringDelimiters, StringSplitOptions.RemoveEmptyEntries);
                switch (buf[0])
                {
                    case 'v': // vertex
                        switch (buf[1])
                        {
                            case ' ':
                            case '\t':
                                if (split.Length != 4) // v + 3 floats
                                    throw new ApplicationException("Parse: Vertex split.Length is invalid");
                                m_positions.Add(float.Parse(split[1]));
                                m_positions.Add(float.Parse(split[2]));
                                m_positions.Add(float.Parse(split[3]));
                                break;
                            case 'n':
                                if (split.Length != 4) // vn + 3 floats
                                    throw new ApplicationException("Parse: Normal split.Length is invalid");
                                m_normals.Add(float.Parse(split[1]));
                                m_normals.Add(float.Parse(split[2]));
                                m_normals.Add(float.Parse(split[3]));
                                break;
                            case 't':
                                if (split.Length != 3) // vt + 2 floats
                                    throw new ApplicationException("Parse: TexCoord split.Length is invalid");
                                m_texcoords.Add(float.Parse(split[1]));
                                m_texcoords.Add(float.Parse(split[2]));
                                break;
                            default:
                                break;
                        }
                        break;

                    case 'f': // face 
                        if (split.Length < 4) // f v/vt/vn v/vt/vn v/vt/vn ...
                            throw new ApplicationException("Parse: Face split.Length is invalid");
                        m_currentFaceSet.Sizes.Add(split.Length - 1);
                        for (int vi = 1; vi < split.Length; vi++)
                        {
                            string[] ptn = split[vi].Split('/');
                            if (ptn.Length == 3) // v/vt/vn or v//vn
                            {
                                m_currentFaceSet.Indices.Add(int.Parse(ptn[2]) - 1);
                                m_currentFaceSet.HasNormals = true;

                                if (!string.IsNullOrEmpty(ptn[1]))
                                {
                                    m_currentFaceSet.Indices.Add(int.Parse(ptn[1]) - 1);
                                    m_currentFaceSet.HasTexCoords = true;
                                }
                            }
                            else if (ptn.Length == 2) // v/vt/
                            {
                                m_currentFaceSet.Indices.Add(int.Parse(ptn[1]) - 1);
                                m_currentFaceSet.HasTexCoords = true;
                            }
                            m_currentFaceSet.Indices.Add(int.Parse(ptn[0]) - 1); // v or v// or v/vt/vn
                        }
                        break;

                    case 'm': // material
                        if (split.Length == 2)
                        {
                            var uri = new Uri(m_resolvedUri, split[1]);
                            string fullPath = Uri.UnescapeDataString(uri.AbsolutePath);

                            m_mtl = new MtlFile { Name = Path.GetFileName(fullPath) };
                            m_mtl.Read(fullPath);
                        }

                        break;

                    case 'g': // group
                    case 'o': // object
                        if (split.Length >= 2) // g + groupName + ...
                        {
                            string curMatName = m_currentFaceSet.MaterialName;

                            // Find group, otherwise create new group
                            if (!m_groups.TryGetValue(split[1], out m_currentGroup))
                            {
                                m_currentGroup = new Group { Name = split[1] };
                                m_groups.Add(m_currentGroup.Name, m_currentGroup);
                                m_currentFaceSet = new FaceSet(curMatName);
                                m_currentGroup.FaceSets.Add(curMatName, m_currentFaceSet);
                            }
                            // Group exists
                            else
                            {
                                // Find faceset with current material, otherwise create new faceset with current material
                                if (!m_currentGroup.FaceSets.TryGetValue(curMatName, out m_currentFaceSet))
                                {
                                    m_currentFaceSet = new FaceSet(curMatName);
                                    m_currentGroup.FaceSets.Add(curMatName, m_currentFaceSet);
                                }
                            }
                        }
                        break;

                    case 'u':
                        if (split.Length != 2) // usemtl + mat name
                            throw new ApplicationException("Parse: Usemtl split.Length is invalid");
                        string matName = split[1];
                        // Find material by name
                        if (m_mtl.Materials.ContainsKey(matName))
                        {
                            // Find faceset with current material, otherwise create new faceset with current material
                            if (!m_currentGroup.FaceSets.TryGetValue(matName, out m_currentFaceSet))
                            {
                                m_currentFaceSet = new FaceSet(matName);
                                m_currentGroup.FaceSets.Add(matName, m_currentFaceSet);
                            }
                        }
                        else
                            Outputs.WriteLine(OutputMessageType.Warning, "Material not found: {0}", matName);
                        break;

                    default:
                        break;
                }
            }

            if (m_groups.Count == 0)
                m_groups.Add(m_currentGroup.Name, m_currentGroup);
            if (m_currentGroup.FaceSets.Count == 0)
                m_currentGroup.FaceSets[m_currentFaceSet.MaterialName] = m_currentFaceSet;

            // Remove empty FaceSets and mark empty Groups
            var delGrpList = new List<string>();
            foreach (Group grp in m_groups.Values)
            {
                var delList = new List<string>();

                foreach (FaceSet fs in grp.FaceSets.Values)
                {
                    if (fs.Indices.Count == 0)
                        delList.Add(fs.MaterialName);
                }
                foreach (string fsName in delList)
                    grp.FaceSets.Remove(fsName);
                if (grp.FaceSets.Count == 0)
                    delGrpList.Add(grp.Name);
            }

            // Remove empty groups
            foreach (string grpn in delGrpList)
                m_groups.Remove(grpn);
        }

        private MtlFile m_mtl = new MtlFile();
        private readonly Dictionary<string, Group> m_groups = new Dictionary<string, Group>();

        private readonly List<float> m_positions = new List<float>();
        private readonly List<float> m_normals = new List<float>();
        private readonly List<float> m_texcoords = new List<float>();

        private Group m_currentGroup = new Group();
        private FaceSet m_currentFaceSet = new FaceSet();
        private Uri m_resolvedUri;

        private static readonly char[] s_stringDelimiters = new[] { ' ', '\t' };
    }
}
