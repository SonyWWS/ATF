// -------------------------------------------------------------------------------------------------------------------
// Generated code, do not edit
// Command Line:  DomGen "collada.xsd" "Schema.cs" "http://www.collada.org/2005/11/COLLADASchema" "Sce.Atf.Collada"
// -------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

using Sce.Atf.Dom;

namespace Sce.Atf.Collada
{
    public static class Schema
    {
        public const string NS = "http://www.collada.org/2005/11/COLLADASchema";

        public static void Initialize(XmlSchemaTypeCollection typeCollection)
        {
            Initialize((ns,name)=>typeCollection.GetNodeType(ns,name),
                (ns,name)=>typeCollection.GetRootElement(ns,name));
        }

        public static void Initialize(IDictionary<string, XmlSchemaTypeCollection> typeCollections)
        {
            Initialize((ns,name)=>typeCollections[ns].GetNodeType(name),
                (ns,name)=>typeCollections[ns].GetRootElement(name));
        }

        private static void Initialize(Func<string, string, DomNodeType> getNodeType, Func<string, string, ChildInfo> getRootElement)
        {
            COLLADA.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "COLLADA");
            COLLADA.versionAttribute = COLLADA.Type.GetAttributeInfo("version");
            COLLADA.assetChild = COLLADA.Type.GetChildInfo("asset");
            COLLADA.library_animationsChild = COLLADA.Type.GetChildInfo("library_animations");
            COLLADA.library_animation_clipsChild = COLLADA.Type.GetChildInfo("library_animation_clips");
            COLLADA.library_camerasChild = COLLADA.Type.GetChildInfo("library_cameras");
            COLLADA.library_controllersChild = COLLADA.Type.GetChildInfo("library_controllers");
            COLLADA.library_geometriesChild = COLLADA.Type.GetChildInfo("library_geometries");
            COLLADA.library_effectsChild = COLLADA.Type.GetChildInfo("library_effects");
            COLLADA.library_force_fieldsChild = COLLADA.Type.GetChildInfo("library_force_fields");
            COLLADA.library_imagesChild = COLLADA.Type.GetChildInfo("library_images");
            COLLADA.library_lightsChild = COLLADA.Type.GetChildInfo("library_lights");
            COLLADA.library_materialsChild = COLLADA.Type.GetChildInfo("library_materials");
            COLLADA.library_nodesChild = COLLADA.Type.GetChildInfo("library_nodes");
            COLLADA.library_physics_materialsChild = COLLADA.Type.GetChildInfo("library_physics_materials");
            COLLADA.library_physics_modelsChild = COLLADA.Type.GetChildInfo("library_physics_models");
            COLLADA.library_physics_scenesChild = COLLADA.Type.GetChildInfo("library_physics_scenes");
            COLLADA.library_visual_scenesChild = COLLADA.Type.GetChildInfo("library_visual_scenes");
            COLLADA.sceneChild = COLLADA.Type.GetChildInfo("scene");
            COLLADA.extraChild = COLLADA.Type.GetChildInfo("extra");

            asset.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "asset");
            asset.createdAttribute = asset.Type.GetAttributeInfo("created");
            asset.keywordsAttribute = asset.Type.GetAttributeInfo("keywords");
            asset.modifiedAttribute = asset.Type.GetAttributeInfo("modified");
            asset.revisionAttribute = asset.Type.GetAttributeInfo("revision");
            asset.subjectAttribute = asset.Type.GetAttributeInfo("subject");
            asset.titleAttribute = asset.Type.GetAttributeInfo("title");
            asset.up_axisAttribute = asset.Type.GetAttributeInfo("up_axis");
            asset.contributorChild = asset.Type.GetChildInfo("contributor");
            asset.unitChild = asset.Type.GetChildInfo("unit");

            asset_contributor.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "asset_contributor");
            asset_contributor.authorAttribute = asset_contributor.Type.GetAttributeInfo("author");
            asset_contributor.authoring_toolAttribute = asset_contributor.Type.GetAttributeInfo("authoring_tool");
            asset_contributor.commentsAttribute = asset_contributor.Type.GetAttributeInfo("comments");
            asset_contributor.copyrightAttribute = asset_contributor.Type.GetAttributeInfo("copyright");
            asset_contributor.source_dataAttribute = asset_contributor.Type.GetAttributeInfo("source_data");

            asset_unit.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "asset_unit");
            asset_unit.meterAttribute = asset_unit.Type.GetAttributeInfo("meter");
            asset_unit.nameAttribute = asset_unit.Type.GetAttributeInfo("name");

            library_animations.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "library_animations");
            library_animations.idAttribute = library_animations.Type.GetAttributeInfo("id");
            library_animations.nameAttribute = library_animations.Type.GetAttributeInfo("name");
            library_animations.assetChild = library_animations.Type.GetChildInfo("asset");
            library_animations.animationChild = library_animations.Type.GetChildInfo("animation");
            library_animations.extraChild = library_animations.Type.GetChildInfo("extra");

            animation.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "animation");
            animation.idAttribute = animation.Type.GetAttributeInfo("id");
            animation.nameAttribute = animation.Type.GetAttributeInfo("name");
            animation.assetChild = animation.Type.GetChildInfo("asset");
            animation.sourceChild = animation.Type.GetChildInfo("source");
            animation.samplerChild = animation.Type.GetChildInfo("sampler");
            animation.channelChild = animation.Type.GetChildInfo("channel");
            animation.animationChild = animation.Type.GetChildInfo("animation");
            animation.extraChild = animation.Type.GetChildInfo("extra");

            source.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "source");
            source.idAttribute = source.Type.GetAttributeInfo("id");
            source.nameAttribute = source.Type.GetAttributeInfo("name");
            source.assetChild = source.Type.GetChildInfo("asset");
            source.IDREF_arrayChild = source.Type.GetChildInfo("IDREF_array");
            source.Name_arrayChild = source.Type.GetChildInfo("Name_array");
            source.bool_arrayChild = source.Type.GetChildInfo("bool_array");
            source.float_arrayChild = source.Type.GetChildInfo("float_array");
            source.int_arrayChild = source.Type.GetChildInfo("int_array");
            source.technique_commonChild = source.Type.GetChildInfo("technique_common");
            source.techniqueChild = source.Type.GetChildInfo("technique");

            IDREF_array.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "IDREF_array");
            IDREF_array.Attribute = IDREF_array.Type.GetAttributeInfo("");
            IDREF_array.idAttribute = IDREF_array.Type.GetAttributeInfo("id");
            IDREF_array.nameAttribute = IDREF_array.Type.GetAttributeInfo("name");
            IDREF_array.countAttribute = IDREF_array.Type.GetAttributeInfo("count");

            Name_array.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "Name_array");
            Name_array.Attribute = Name_array.Type.GetAttributeInfo("");
            Name_array.idAttribute = Name_array.Type.GetAttributeInfo("id");
            Name_array.nameAttribute = Name_array.Type.GetAttributeInfo("name");
            Name_array.countAttribute = Name_array.Type.GetAttributeInfo("count");

            bool_array.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "bool_array");
            bool_array.Attribute = bool_array.Type.GetAttributeInfo("");
            bool_array.idAttribute = bool_array.Type.GetAttributeInfo("id");
            bool_array.nameAttribute = bool_array.Type.GetAttributeInfo("name");
            bool_array.countAttribute = bool_array.Type.GetAttributeInfo("count");

            float_array.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "float_array");
            float_array.Attribute = float_array.Type.GetAttributeInfo("");
            float_array.idAttribute = float_array.Type.GetAttributeInfo("id");
            float_array.nameAttribute = float_array.Type.GetAttributeInfo("name");
            float_array.countAttribute = float_array.Type.GetAttributeInfo("count");
            float_array.digitsAttribute = float_array.Type.GetAttributeInfo("digits");
            float_array.magnitudeAttribute = float_array.Type.GetAttributeInfo("magnitude");

            int_array.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "int_array");
            int_array.Attribute = int_array.Type.GetAttributeInfo("");
            int_array.idAttribute = int_array.Type.GetAttributeInfo("id");
            int_array.nameAttribute = int_array.Type.GetAttributeInfo("name");
            int_array.countAttribute = int_array.Type.GetAttributeInfo("count");
            int_array.minInclusiveAttribute = int_array.Type.GetAttributeInfo("minInclusive");
            int_array.maxInclusiveAttribute = int_array.Type.GetAttributeInfo("maxInclusive");

            source_technique_common.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "source_technique_common");
            source_technique_common.accessorChild = source_technique_common.Type.GetChildInfo("accessor");

            accessor.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "accessor");
            accessor.countAttribute = accessor.Type.GetAttributeInfo("count");
            accessor.offsetAttribute = accessor.Type.GetAttributeInfo("offset");
            accessor.sourceAttribute = accessor.Type.GetAttributeInfo("source");
            accessor.strideAttribute = accessor.Type.GetAttributeInfo("stride");
            accessor.paramChild = accessor.Type.GetChildInfo("param");

            param.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "param");
            param.Attribute = param.Type.GetAttributeInfo("");
            param.nameAttribute = param.Type.GetAttributeInfo("name");
            param.sidAttribute = param.Type.GetAttributeInfo("sid");
            param.semanticAttribute = param.Type.GetAttributeInfo("semantic");
            param.typeAttribute = param.Type.GetAttributeInfo("type");

            technique.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "technique");
            technique.profileAttribute = technique.Type.GetAttributeInfo("profile");

            sampler.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "sampler");
            sampler.idAttribute = sampler.Type.GetAttributeInfo("id");
            sampler.inputChild = sampler.Type.GetChildInfo("input");

            InputLocal.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "InputLocal");
            InputLocal.semanticAttribute = InputLocal.Type.GetAttributeInfo("semantic");
            InputLocal.sourceAttribute = InputLocal.Type.GetAttributeInfo("source");

            channel.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "channel");
            channel.sourceAttribute = channel.Type.GetAttributeInfo("source");
            channel.targetAttribute = channel.Type.GetAttributeInfo("target");

            extra.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "extra");
            extra.idAttribute = extra.Type.GetAttributeInfo("id");
            extra.nameAttribute = extra.Type.GetAttributeInfo("name");
            extra.typeAttribute = extra.Type.GetAttributeInfo("type");
            extra.assetChild = extra.Type.GetChildInfo("asset");
            extra.techniqueChild = extra.Type.GetChildInfo("technique");

            library_animation_clips.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "library_animation_clips");
            library_animation_clips.idAttribute = library_animation_clips.Type.GetAttributeInfo("id");
            library_animation_clips.nameAttribute = library_animation_clips.Type.GetAttributeInfo("name");
            library_animation_clips.assetChild = library_animation_clips.Type.GetChildInfo("asset");
            library_animation_clips.animation_clipChild = library_animation_clips.Type.GetChildInfo("animation_clip");
            library_animation_clips.extraChild = library_animation_clips.Type.GetChildInfo("extra");

            animation_clip.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "animation_clip");
            animation_clip.idAttribute = animation_clip.Type.GetAttributeInfo("id");
            animation_clip.nameAttribute = animation_clip.Type.GetAttributeInfo("name");
            animation_clip.startAttribute = animation_clip.Type.GetAttributeInfo("start");
            animation_clip.endAttribute = animation_clip.Type.GetAttributeInfo("end");
            animation_clip.assetChild = animation_clip.Type.GetChildInfo("asset");
            animation_clip.instance_animationChild = animation_clip.Type.GetChildInfo("instance_animation");
            animation_clip.extraChild = animation_clip.Type.GetChildInfo("extra");

            InstanceWithExtra.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "InstanceWithExtra");
            InstanceWithExtra.urlAttribute = InstanceWithExtra.Type.GetAttributeInfo("url");
            InstanceWithExtra.sidAttribute = InstanceWithExtra.Type.GetAttributeInfo("sid");
            InstanceWithExtra.nameAttribute = InstanceWithExtra.Type.GetAttributeInfo("name");
            InstanceWithExtra.extraChild = InstanceWithExtra.Type.GetChildInfo("extra");

            library_cameras.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "library_cameras");
            library_cameras.idAttribute = library_cameras.Type.GetAttributeInfo("id");
            library_cameras.nameAttribute = library_cameras.Type.GetAttributeInfo("name");
            library_cameras.assetChild = library_cameras.Type.GetChildInfo("asset");
            library_cameras.cameraChild = library_cameras.Type.GetChildInfo("camera");
            library_cameras.extraChild = library_cameras.Type.GetChildInfo("extra");

            camera.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "camera");
            camera.idAttribute = camera.Type.GetAttributeInfo("id");
            camera.nameAttribute = camera.Type.GetAttributeInfo("name");
            camera.assetChild = camera.Type.GetChildInfo("asset");
            camera.opticsChild = camera.Type.GetChildInfo("optics");
            camera.imagerChild = camera.Type.GetChildInfo("imager");
            camera.extraChild = camera.Type.GetChildInfo("extra");

            camera_optics.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "camera_optics");
            camera_optics.technique_commonChild = camera_optics.Type.GetChildInfo("technique_common");
            camera_optics.techniqueChild = camera_optics.Type.GetChildInfo("technique");
            camera_optics.extraChild = camera_optics.Type.GetChildInfo("extra");

            optics_technique_common.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "optics_technique_common");
            optics_technique_common.orthographicChild = optics_technique_common.Type.GetChildInfo("orthographic");
            optics_technique_common.perspectiveChild = optics_technique_common.Type.GetChildInfo("perspective");

            technique_common_orthographic.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "technique_common_orthographic");
            technique_common_orthographic.xmagChild = technique_common_orthographic.Type.GetChildInfo("xmag");
            technique_common_orthographic.ymagChild = technique_common_orthographic.Type.GetChildInfo("ymag");
            technique_common_orthographic.aspect_ratioChild = technique_common_orthographic.Type.GetChildInfo("aspect_ratio");
            technique_common_orthographic.znearChild = technique_common_orthographic.Type.GetChildInfo("znear");
            technique_common_orthographic.zfarChild = technique_common_orthographic.Type.GetChildInfo("zfar");

            TargetableFloat.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "TargetableFloat");
            TargetableFloat.Attribute = TargetableFloat.Type.GetAttributeInfo("");
            TargetableFloat.sidAttribute = TargetableFloat.Type.GetAttributeInfo("sid");

            technique_common_perspective.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "technique_common_perspective");
            technique_common_perspective.xfovChild = technique_common_perspective.Type.GetChildInfo("xfov");
            technique_common_perspective.yfovChild = technique_common_perspective.Type.GetChildInfo("yfov");
            technique_common_perspective.aspect_ratioChild = technique_common_perspective.Type.GetChildInfo("aspect_ratio");
            technique_common_perspective.znearChild = technique_common_perspective.Type.GetChildInfo("znear");
            technique_common_perspective.zfarChild = technique_common_perspective.Type.GetChildInfo("zfar");

            camera_imager.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "camera_imager");
            camera_imager.techniqueChild = camera_imager.Type.GetChildInfo("technique");
            camera_imager.extraChild = camera_imager.Type.GetChildInfo("extra");

            library_controllers.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "library_controllers");
            library_controllers.idAttribute = library_controllers.Type.GetAttributeInfo("id");
            library_controllers.nameAttribute = library_controllers.Type.GetAttributeInfo("name");
            library_controllers.assetChild = library_controllers.Type.GetChildInfo("asset");
            library_controllers.controllerChild = library_controllers.Type.GetChildInfo("controller");
            library_controllers.extraChild = library_controllers.Type.GetChildInfo("extra");

            controller.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "controller");
            controller.idAttribute = controller.Type.GetAttributeInfo("id");
            controller.nameAttribute = controller.Type.GetAttributeInfo("name");
            controller.assetChild = controller.Type.GetChildInfo("asset");
            controller.skinChild = controller.Type.GetChildInfo("skin");
            controller.morphChild = controller.Type.GetChildInfo("morph");
            controller.extraChild = controller.Type.GetChildInfo("extra");

            skin.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "skin");
            skin.bind_shape_matrixAttribute = skin.Type.GetAttributeInfo("bind_shape_matrix");
            skin.sourceAttribute = skin.Type.GetAttributeInfo("source");
            skin.sourceChild = skin.Type.GetChildInfo("source");
            skin.jointsChild = skin.Type.GetChildInfo("joints");
            skin.vertex_weightsChild = skin.Type.GetChildInfo("vertex_weights");
            skin.extraChild = skin.Type.GetChildInfo("extra");

            skin_joints.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "skin_joints");
            skin_joints.inputChild = skin_joints.Type.GetChildInfo("input");
            skin_joints.extraChild = skin_joints.Type.GetChildInfo("extra");

            skin_vertex_weights.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "skin_vertex_weights");
            skin_vertex_weights.vcountAttribute = skin_vertex_weights.Type.GetAttributeInfo("vcount");
            skin_vertex_weights.vAttribute = skin_vertex_weights.Type.GetAttributeInfo("v");
            skin_vertex_weights.countAttribute = skin_vertex_weights.Type.GetAttributeInfo("count");
            skin_vertex_weights.inputChild = skin_vertex_weights.Type.GetChildInfo("input");
            skin_vertex_weights.extraChild = skin_vertex_weights.Type.GetChildInfo("extra");

            InputLocalOffset.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "InputLocalOffset");
            InputLocalOffset.offsetAttribute = InputLocalOffset.Type.GetAttributeInfo("offset");
            InputLocalOffset.semanticAttribute = InputLocalOffset.Type.GetAttributeInfo("semantic");
            InputLocalOffset.sourceAttribute = InputLocalOffset.Type.GetAttributeInfo("source");
            InputLocalOffset.setAttribute = InputLocalOffset.Type.GetAttributeInfo("set");

            morph.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "morph");
            morph.methodAttribute = morph.Type.GetAttributeInfo("method");
            morph.sourceAttribute = morph.Type.GetAttributeInfo("source");
            morph.sourceChild = morph.Type.GetChildInfo("source");
            morph.targetsChild = morph.Type.GetChildInfo("targets");
            morph.extraChild = morph.Type.GetChildInfo("extra");

            morph_targets.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "morph_targets");
            morph_targets.inputChild = morph_targets.Type.GetChildInfo("input");
            morph_targets.extraChild = morph_targets.Type.GetChildInfo("extra");

            library_geometries.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "library_geometries");
            library_geometries.idAttribute = library_geometries.Type.GetAttributeInfo("id");
            library_geometries.nameAttribute = library_geometries.Type.GetAttributeInfo("name");
            library_geometries.assetChild = library_geometries.Type.GetChildInfo("asset");
            library_geometries.geometryChild = library_geometries.Type.GetChildInfo("geometry");
            library_geometries.extraChild = library_geometries.Type.GetChildInfo("extra");

            geometry.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "geometry");
            geometry.idAttribute = geometry.Type.GetAttributeInfo("id");
            geometry.nameAttribute = geometry.Type.GetAttributeInfo("name");
            geometry.assetChild = geometry.Type.GetChildInfo("asset");
            geometry.convex_meshChild = geometry.Type.GetChildInfo("convex_mesh");
            geometry.meshChild = geometry.Type.GetChildInfo("mesh");
            geometry.splineChild = geometry.Type.GetChildInfo("spline");
            geometry.extraChild = geometry.Type.GetChildInfo("extra");

            convex_mesh.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "convex_mesh");
            convex_mesh.convex_hull_ofAttribute = convex_mesh.Type.GetAttributeInfo("convex_hull_of");
            convex_mesh.sourceChild = convex_mesh.Type.GetChildInfo("source");
            convex_mesh.verticesChild = convex_mesh.Type.GetChildInfo("vertices");
            convex_mesh.linesChild = convex_mesh.Type.GetChildInfo("lines");
            convex_mesh.linestripsChild = convex_mesh.Type.GetChildInfo("linestrips");
            convex_mesh.polygonsChild = convex_mesh.Type.GetChildInfo("polygons");
            convex_mesh.polylistChild = convex_mesh.Type.GetChildInfo("polylist");
            convex_mesh.trianglesChild = convex_mesh.Type.GetChildInfo("triangles");
            convex_mesh.trifansChild = convex_mesh.Type.GetChildInfo("trifans");
            convex_mesh.tristripsChild = convex_mesh.Type.GetChildInfo("tristrips");
            convex_mesh.extraChild = convex_mesh.Type.GetChildInfo("extra");

            vertices.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "vertices");
            vertices.idAttribute = vertices.Type.GetAttributeInfo("id");
            vertices.nameAttribute = vertices.Type.GetAttributeInfo("name");
            vertices.inputChild = vertices.Type.GetChildInfo("input");
            vertices.extraChild = vertices.Type.GetChildInfo("extra");

            lines.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "lines");
            lines.pAttribute = lines.Type.GetAttributeInfo("p");
            lines.nameAttribute = lines.Type.GetAttributeInfo("name");
            lines.countAttribute = lines.Type.GetAttributeInfo("count");
            lines.materialAttribute = lines.Type.GetAttributeInfo("material");
            lines.inputChild = lines.Type.GetChildInfo("input");
            lines.extraChild = lines.Type.GetChildInfo("extra");

            linestrips.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "linestrips");
            linestrips.nameAttribute = linestrips.Type.GetAttributeInfo("name");
            linestrips.countAttribute = linestrips.Type.GetAttributeInfo("count");
            linestrips.materialAttribute = linestrips.Type.GetAttributeInfo("material");
            linestrips.inputChild = linestrips.Type.GetChildInfo("input");
            linestrips.pChild = linestrips.Type.GetChildInfo("p");
            linestrips.extraChild = linestrips.Type.GetChildInfo("extra");

            ListOfUInts.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "ListOfUInts");
            ListOfUInts.Attribute = ListOfUInts.Type.GetAttributeInfo("");

            polygons.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "polygons");
            polygons.pAttribute = polygons.Type.GetAttributeInfo("p");
            polygons.nameAttribute = polygons.Type.GetAttributeInfo("name");
            polygons.countAttribute = polygons.Type.GetAttributeInfo("count");
            polygons.materialAttribute = polygons.Type.GetAttributeInfo("material");
            polygons.inputChild = polygons.Type.GetChildInfo("input");
            polygons.phChild = polygons.Type.GetChildInfo("ph");
            polygons.extraChild = polygons.Type.GetChildInfo("extra");

            polygons_ph.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "polygons_ph");
            polygons_ph.pAttribute = polygons_ph.Type.GetAttributeInfo("p");
            polygons_ph.hChild = polygons_ph.Type.GetChildInfo("h");

            polylist.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "polylist");
            polylist.vcountAttribute = polylist.Type.GetAttributeInfo("vcount");
            polylist.pAttribute = polylist.Type.GetAttributeInfo("p");
            polylist.nameAttribute = polylist.Type.GetAttributeInfo("name");
            polylist.countAttribute = polylist.Type.GetAttributeInfo("count");
            polylist.materialAttribute = polylist.Type.GetAttributeInfo("material");
            polylist.inputChild = polylist.Type.GetChildInfo("input");
            polylist.extraChild = polylist.Type.GetChildInfo("extra");

            triangles.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "triangles");
            triangles.pAttribute = triangles.Type.GetAttributeInfo("p");
            triangles.nameAttribute = triangles.Type.GetAttributeInfo("name");
            triangles.countAttribute = triangles.Type.GetAttributeInfo("count");
            triangles.materialAttribute = triangles.Type.GetAttributeInfo("material");
            triangles.inputChild = triangles.Type.GetChildInfo("input");
            triangles.extraChild = triangles.Type.GetChildInfo("extra");

            trifans.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "trifans");
            trifans.nameAttribute = trifans.Type.GetAttributeInfo("name");
            trifans.countAttribute = trifans.Type.GetAttributeInfo("count");
            trifans.materialAttribute = trifans.Type.GetAttributeInfo("material");
            trifans.inputChild = trifans.Type.GetChildInfo("input");
            trifans.pChild = trifans.Type.GetChildInfo("p");
            trifans.extraChild = trifans.Type.GetChildInfo("extra");

            tristrips.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "tristrips");
            tristrips.nameAttribute = tristrips.Type.GetAttributeInfo("name");
            tristrips.countAttribute = tristrips.Type.GetAttributeInfo("count");
            tristrips.materialAttribute = tristrips.Type.GetAttributeInfo("material");
            tristrips.inputChild = tristrips.Type.GetChildInfo("input");
            tristrips.pChild = tristrips.Type.GetChildInfo("p");
            tristrips.extraChild = tristrips.Type.GetChildInfo("extra");

            mesh.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "mesh");
            mesh.sourceChild = mesh.Type.GetChildInfo("source");
            mesh.verticesChild = mesh.Type.GetChildInfo("vertices");
            mesh.linesChild = mesh.Type.GetChildInfo("lines");
            mesh.linestripsChild = mesh.Type.GetChildInfo("linestrips");
            mesh.polygonsChild = mesh.Type.GetChildInfo("polygons");
            mesh.polylistChild = mesh.Type.GetChildInfo("polylist");
            mesh.trianglesChild = mesh.Type.GetChildInfo("triangles");
            mesh.trifansChild = mesh.Type.GetChildInfo("trifans");
            mesh.tristripsChild = mesh.Type.GetChildInfo("tristrips");
            mesh.extraChild = mesh.Type.GetChildInfo("extra");

            spline.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "spline");
            spline.closedAttribute = spline.Type.GetAttributeInfo("closed");
            spline.sourceChild = spline.Type.GetChildInfo("source");
            spline.control_verticesChild = spline.Type.GetChildInfo("control_vertices");
            spline.extraChild = spline.Type.GetChildInfo("extra");

            spline_control_vertices.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "spline_control_vertices");
            spline_control_vertices.inputChild = spline_control_vertices.Type.GetChildInfo("input");
            spline_control_vertices.extraChild = spline_control_vertices.Type.GetChildInfo("extra");

            library_effects.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "library_effects");
            library_effects.idAttribute = library_effects.Type.GetAttributeInfo("id");
            library_effects.nameAttribute = library_effects.Type.GetAttributeInfo("name");
            library_effects.assetChild = library_effects.Type.GetChildInfo("asset");
            library_effects.effectChild = library_effects.Type.GetChildInfo("effect");
            library_effects.extraChild = library_effects.Type.GetChildInfo("extra");

            effect.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "effect");
            effect.idAttribute = effect.Type.GetAttributeInfo("id");
            effect.nameAttribute = effect.Type.GetAttributeInfo("name");
            effect.assetChild = effect.Type.GetChildInfo("asset");
            effect.annotateChild = effect.Type.GetChildInfo("annotate");
            effect.imageChild = effect.Type.GetChildInfo("image");
            effect.newparamChild = effect.Type.GetChildInfo("newparam");
            effect.fx_profile_abstractChild = effect.Type.GetChildInfo("fx_profile_abstract");
            effect.profile_CGChild = effect.Type.GetChildInfo("profile_CG");
            effect.profile_COMMONChild = effect.Type.GetChildInfo("profile_COMMON");
            effect.extraChild = effect.Type.GetChildInfo("extra");

            fx_annotate_common.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "fx_annotate_common");
            fx_annotate_common.boolAttribute = fx_annotate_common.Type.GetAttributeInfo("bool");
            fx_annotate_common.bool2Attribute = fx_annotate_common.Type.GetAttributeInfo("bool2");
            fx_annotate_common.bool3Attribute = fx_annotate_common.Type.GetAttributeInfo("bool3");
            fx_annotate_common.bool4Attribute = fx_annotate_common.Type.GetAttributeInfo("bool4");
            fx_annotate_common.intAttribute = fx_annotate_common.Type.GetAttributeInfo("int");
            fx_annotate_common.int2Attribute = fx_annotate_common.Type.GetAttributeInfo("int2");
            fx_annotate_common.int3Attribute = fx_annotate_common.Type.GetAttributeInfo("int3");
            fx_annotate_common.int4Attribute = fx_annotate_common.Type.GetAttributeInfo("int4");
            fx_annotate_common.floatAttribute = fx_annotate_common.Type.GetAttributeInfo("float");
            fx_annotate_common.float2Attribute = fx_annotate_common.Type.GetAttributeInfo("float2");
            fx_annotate_common.float3Attribute = fx_annotate_common.Type.GetAttributeInfo("float3");
            fx_annotate_common.float4Attribute = fx_annotate_common.Type.GetAttributeInfo("float4");
            fx_annotate_common.float2x2Attribute = fx_annotate_common.Type.GetAttributeInfo("float2x2");
            fx_annotate_common.float3x3Attribute = fx_annotate_common.Type.GetAttributeInfo("float3x3");
            fx_annotate_common.float4x4Attribute = fx_annotate_common.Type.GetAttributeInfo("float4x4");
            fx_annotate_common.stringAttribute = fx_annotate_common.Type.GetAttributeInfo("string");
            fx_annotate_common.nameAttribute = fx_annotate_common.Type.GetAttributeInfo("name");

            image.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "image");
            image.dataAttribute = image.Type.GetAttributeInfo("data");
            image.init_fromAttribute = image.Type.GetAttributeInfo("init_from");
            image.idAttribute = image.Type.GetAttributeInfo("id");
            image.nameAttribute = image.Type.GetAttributeInfo("name");
            image.formatAttribute = image.Type.GetAttributeInfo("format");
            image.heightAttribute = image.Type.GetAttributeInfo("height");
            image.widthAttribute = image.Type.GetAttributeInfo("width");
            image.depthAttribute = image.Type.GetAttributeInfo("depth");
            image.assetChild = image.Type.GetChildInfo("asset");
            image.extraChild = image.Type.GetChildInfo("extra");

            fx_newparam_common.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "fx_newparam_common");
            fx_newparam_common.semanticAttribute = fx_newparam_common.Type.GetAttributeInfo("semantic");
            fx_newparam_common.modifierAttribute = fx_newparam_common.Type.GetAttributeInfo("modifier");
            fx_newparam_common.boolAttribute = fx_newparam_common.Type.GetAttributeInfo("bool");
            fx_newparam_common.bool2Attribute = fx_newparam_common.Type.GetAttributeInfo("bool2");
            fx_newparam_common.bool3Attribute = fx_newparam_common.Type.GetAttributeInfo("bool3");
            fx_newparam_common.bool4Attribute = fx_newparam_common.Type.GetAttributeInfo("bool4");
            fx_newparam_common.intAttribute = fx_newparam_common.Type.GetAttributeInfo("int");
            fx_newparam_common.int2Attribute = fx_newparam_common.Type.GetAttributeInfo("int2");
            fx_newparam_common.int3Attribute = fx_newparam_common.Type.GetAttributeInfo("int3");
            fx_newparam_common.int4Attribute = fx_newparam_common.Type.GetAttributeInfo("int4");
            fx_newparam_common.floatAttribute = fx_newparam_common.Type.GetAttributeInfo("float");
            fx_newparam_common.float2Attribute = fx_newparam_common.Type.GetAttributeInfo("float2");
            fx_newparam_common.float3Attribute = fx_newparam_common.Type.GetAttributeInfo("float3");
            fx_newparam_common.float4Attribute = fx_newparam_common.Type.GetAttributeInfo("float4");
            fx_newparam_common.float1x1Attribute = fx_newparam_common.Type.GetAttributeInfo("float1x1");
            fx_newparam_common.float1x2Attribute = fx_newparam_common.Type.GetAttributeInfo("float1x2");
            fx_newparam_common.float1x3Attribute = fx_newparam_common.Type.GetAttributeInfo("float1x3");
            fx_newparam_common.float1x4Attribute = fx_newparam_common.Type.GetAttributeInfo("float1x4");
            fx_newparam_common.float2x1Attribute = fx_newparam_common.Type.GetAttributeInfo("float2x1");
            fx_newparam_common.float2x2Attribute = fx_newparam_common.Type.GetAttributeInfo("float2x2");
            fx_newparam_common.float2x3Attribute = fx_newparam_common.Type.GetAttributeInfo("float2x3");
            fx_newparam_common.float2x4Attribute = fx_newparam_common.Type.GetAttributeInfo("float2x4");
            fx_newparam_common.float3x1Attribute = fx_newparam_common.Type.GetAttributeInfo("float3x1");
            fx_newparam_common.float3x2Attribute = fx_newparam_common.Type.GetAttributeInfo("float3x2");
            fx_newparam_common.float3x3Attribute = fx_newparam_common.Type.GetAttributeInfo("float3x3");
            fx_newparam_common.float3x4Attribute = fx_newparam_common.Type.GetAttributeInfo("float3x4");
            fx_newparam_common.float4x1Attribute = fx_newparam_common.Type.GetAttributeInfo("float4x1");
            fx_newparam_common.float4x2Attribute = fx_newparam_common.Type.GetAttributeInfo("float4x2");
            fx_newparam_common.float4x3Attribute = fx_newparam_common.Type.GetAttributeInfo("float4x3");
            fx_newparam_common.float4x4Attribute = fx_newparam_common.Type.GetAttributeInfo("float4x4");
            fx_newparam_common.enumAttribute = fx_newparam_common.Type.GetAttributeInfo("enum");
            fx_newparam_common.sidAttribute = fx_newparam_common.Type.GetAttributeInfo("sid");
            fx_newparam_common.annotateChild = fx_newparam_common.Type.GetChildInfo("annotate");
            fx_newparam_common.surfaceChild = fx_newparam_common.Type.GetChildInfo("surface");
            fx_newparam_common.sampler1DChild = fx_newparam_common.Type.GetChildInfo("sampler1D");
            fx_newparam_common.sampler2DChild = fx_newparam_common.Type.GetChildInfo("sampler2D");
            fx_newparam_common.sampler3DChild = fx_newparam_common.Type.GetChildInfo("sampler3D");
            fx_newparam_common.samplerCUBEChild = fx_newparam_common.Type.GetChildInfo("samplerCUBE");
            fx_newparam_common.samplerRECTChild = fx_newparam_common.Type.GetChildInfo("samplerRECT");
            fx_newparam_common.samplerDEPTHChild = fx_newparam_common.Type.GetChildInfo("samplerDEPTH");

            fx_surface_common.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "fx_surface_common");
            fx_surface_common.formatAttribute = fx_surface_common.Type.GetAttributeInfo("format");
            fx_surface_common.sizeAttribute = fx_surface_common.Type.GetAttributeInfo("size");
            fx_surface_common.viewport_ratioAttribute = fx_surface_common.Type.GetAttributeInfo("viewport_ratio");
            fx_surface_common.mip_levelsAttribute = fx_surface_common.Type.GetAttributeInfo("mip_levels");
            fx_surface_common.mipmap_generateAttribute = fx_surface_common.Type.GetAttributeInfo("mipmap_generate");
            fx_surface_common.typeAttribute = fx_surface_common.Type.GetAttributeInfo("type");
            fx_surface_common.init_as_nullChild = fx_surface_common.Type.GetChildInfo("init_as_null");
            fx_surface_common.init_as_targetChild = fx_surface_common.Type.GetChildInfo("init_as_target");
            fx_surface_common.init_cubeChild = fx_surface_common.Type.GetChildInfo("init_cube");
            fx_surface_common.init_volumeChild = fx_surface_common.Type.GetChildInfo("init_volume");
            fx_surface_common.init_planarChild = fx_surface_common.Type.GetChildInfo("init_planar");
            fx_surface_common.init_fromChild = fx_surface_common.Type.GetChildInfo("init_from");
            fx_surface_common.format_hintChild = fx_surface_common.Type.GetChildInfo("format_hint");
            fx_surface_common.extraChild = fx_surface_common.Type.GetChildInfo("extra");

            fx_surface_init_cube_common.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "fx_surface_init_cube_common");
            fx_surface_init_cube_common.allChild = fx_surface_init_cube_common.Type.GetChildInfo("all");
            fx_surface_init_cube_common.primaryChild = fx_surface_init_cube_common.Type.GetChildInfo("primary");
            fx_surface_init_cube_common.faceChild = fx_surface_init_cube_common.Type.GetChildInfo("face");

            fx_surface_init_cube_common_all.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "fx_surface_init_cube_common_all");
            fx_surface_init_cube_common_all.refAttribute = fx_surface_init_cube_common_all.Type.GetAttributeInfo("ref");

            fx_surface_init_cube_common_primary.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "fx_surface_init_cube_common_primary");
            fx_surface_init_cube_common_primary.refAttribute = fx_surface_init_cube_common_primary.Type.GetAttributeInfo("ref");
            fx_surface_init_cube_common_primary.orderChild = fx_surface_init_cube_common_primary.Type.GetChildInfo("order");

            fx_surface_face_enum.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "fx_surface_face_enum");
            fx_surface_face_enum.Attribute = fx_surface_face_enum.Type.GetAttributeInfo("");

            fx_surface_init_cube_common_face.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "fx_surface_init_cube_common_face");
            fx_surface_init_cube_common_face.refAttribute = fx_surface_init_cube_common_face.Type.GetAttributeInfo("ref");

            fx_surface_init_volume_common.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "fx_surface_init_volume_common");
            fx_surface_init_volume_common.allChild = fx_surface_init_volume_common.Type.GetChildInfo("all");
            fx_surface_init_volume_common.primaryChild = fx_surface_init_volume_common.Type.GetChildInfo("primary");

            fx_surface_init_volume_common_all.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "fx_surface_init_volume_common_all");
            fx_surface_init_volume_common_all.refAttribute = fx_surface_init_volume_common_all.Type.GetAttributeInfo("ref");

            fx_surface_init_volume_common_primary.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "fx_surface_init_volume_common_primary");
            fx_surface_init_volume_common_primary.refAttribute = fx_surface_init_volume_common_primary.Type.GetAttributeInfo("ref");

            fx_surface_init_planar_common.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "fx_surface_init_planar_common");
            fx_surface_init_planar_common.allChild = fx_surface_init_planar_common.Type.GetChildInfo("all");

            fx_surface_init_planar_common_all.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "fx_surface_init_planar_common_all");
            fx_surface_init_planar_common_all.refAttribute = fx_surface_init_planar_common_all.Type.GetAttributeInfo("ref");

            fx_surface_init_from_common.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "fx_surface_init_from_common");
            fx_surface_init_from_common.Attribute = fx_surface_init_from_common.Type.GetAttributeInfo("");
            fx_surface_init_from_common.mipAttribute = fx_surface_init_from_common.Type.GetAttributeInfo("mip");
            fx_surface_init_from_common.sliceAttribute = fx_surface_init_from_common.Type.GetAttributeInfo("slice");
            fx_surface_init_from_common.faceAttribute = fx_surface_init_from_common.Type.GetAttributeInfo("face");

            fx_surface_format_hint_common.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "fx_surface_format_hint_common");
            fx_surface_format_hint_common.channelsAttribute = fx_surface_format_hint_common.Type.GetAttributeInfo("channels");
            fx_surface_format_hint_common.rangeAttribute = fx_surface_format_hint_common.Type.GetAttributeInfo("range");
            fx_surface_format_hint_common.precisionAttribute = fx_surface_format_hint_common.Type.GetAttributeInfo("precision");
            fx_surface_format_hint_common.optionChild = fx_surface_format_hint_common.Type.GetChildInfo("option");
            fx_surface_format_hint_common.extraChild = fx_surface_format_hint_common.Type.GetChildInfo("extra");

            fx_surface_format_hint_option_enum.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "fx_surface_format_hint_option_enum");
            fx_surface_format_hint_option_enum.Attribute = fx_surface_format_hint_option_enum.Type.GetAttributeInfo("");

            fx_sampler1D_common.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "fx_sampler1D_common");
            fx_sampler1D_common.sourceAttribute = fx_sampler1D_common.Type.GetAttributeInfo("source");
            fx_sampler1D_common.wrap_sAttribute = fx_sampler1D_common.Type.GetAttributeInfo("wrap_s");
            fx_sampler1D_common.minfilterAttribute = fx_sampler1D_common.Type.GetAttributeInfo("minfilter");
            fx_sampler1D_common.magfilterAttribute = fx_sampler1D_common.Type.GetAttributeInfo("magfilter");
            fx_sampler1D_common.mipfilterAttribute = fx_sampler1D_common.Type.GetAttributeInfo("mipfilter");
            fx_sampler1D_common.border_colorAttribute = fx_sampler1D_common.Type.GetAttributeInfo("border_color");
            fx_sampler1D_common.mipmap_maxlevelAttribute = fx_sampler1D_common.Type.GetAttributeInfo("mipmap_maxlevel");
            fx_sampler1D_common.mipmap_biasAttribute = fx_sampler1D_common.Type.GetAttributeInfo("mipmap_bias");
            fx_sampler1D_common.extraChild = fx_sampler1D_common.Type.GetChildInfo("extra");

            fx_sampler2D_common.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "fx_sampler2D_common");
            fx_sampler2D_common.sourceAttribute = fx_sampler2D_common.Type.GetAttributeInfo("source");
            fx_sampler2D_common.wrap_sAttribute = fx_sampler2D_common.Type.GetAttributeInfo("wrap_s");
            fx_sampler2D_common.wrap_tAttribute = fx_sampler2D_common.Type.GetAttributeInfo("wrap_t");
            fx_sampler2D_common.minfilterAttribute = fx_sampler2D_common.Type.GetAttributeInfo("minfilter");
            fx_sampler2D_common.magfilterAttribute = fx_sampler2D_common.Type.GetAttributeInfo("magfilter");
            fx_sampler2D_common.mipfilterAttribute = fx_sampler2D_common.Type.GetAttributeInfo("mipfilter");
            fx_sampler2D_common.border_colorAttribute = fx_sampler2D_common.Type.GetAttributeInfo("border_color");
            fx_sampler2D_common.mipmap_maxlevelAttribute = fx_sampler2D_common.Type.GetAttributeInfo("mipmap_maxlevel");
            fx_sampler2D_common.mipmap_biasAttribute = fx_sampler2D_common.Type.GetAttributeInfo("mipmap_bias");
            fx_sampler2D_common.extraChild = fx_sampler2D_common.Type.GetChildInfo("extra");

            fx_sampler3D_common.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "fx_sampler3D_common");
            fx_sampler3D_common.sourceAttribute = fx_sampler3D_common.Type.GetAttributeInfo("source");
            fx_sampler3D_common.wrap_sAttribute = fx_sampler3D_common.Type.GetAttributeInfo("wrap_s");
            fx_sampler3D_common.wrap_tAttribute = fx_sampler3D_common.Type.GetAttributeInfo("wrap_t");
            fx_sampler3D_common.wrap_pAttribute = fx_sampler3D_common.Type.GetAttributeInfo("wrap_p");
            fx_sampler3D_common.minfilterAttribute = fx_sampler3D_common.Type.GetAttributeInfo("minfilter");
            fx_sampler3D_common.magfilterAttribute = fx_sampler3D_common.Type.GetAttributeInfo("magfilter");
            fx_sampler3D_common.mipfilterAttribute = fx_sampler3D_common.Type.GetAttributeInfo("mipfilter");
            fx_sampler3D_common.border_colorAttribute = fx_sampler3D_common.Type.GetAttributeInfo("border_color");
            fx_sampler3D_common.mipmap_maxlevelAttribute = fx_sampler3D_common.Type.GetAttributeInfo("mipmap_maxlevel");
            fx_sampler3D_common.mipmap_biasAttribute = fx_sampler3D_common.Type.GetAttributeInfo("mipmap_bias");
            fx_sampler3D_common.extraChild = fx_sampler3D_common.Type.GetChildInfo("extra");

            fx_samplerCUBE_common.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "fx_samplerCUBE_common");
            fx_samplerCUBE_common.sourceAttribute = fx_samplerCUBE_common.Type.GetAttributeInfo("source");
            fx_samplerCUBE_common.wrap_sAttribute = fx_samplerCUBE_common.Type.GetAttributeInfo("wrap_s");
            fx_samplerCUBE_common.wrap_tAttribute = fx_samplerCUBE_common.Type.GetAttributeInfo("wrap_t");
            fx_samplerCUBE_common.wrap_pAttribute = fx_samplerCUBE_common.Type.GetAttributeInfo("wrap_p");
            fx_samplerCUBE_common.minfilterAttribute = fx_samplerCUBE_common.Type.GetAttributeInfo("minfilter");
            fx_samplerCUBE_common.magfilterAttribute = fx_samplerCUBE_common.Type.GetAttributeInfo("magfilter");
            fx_samplerCUBE_common.mipfilterAttribute = fx_samplerCUBE_common.Type.GetAttributeInfo("mipfilter");
            fx_samplerCUBE_common.border_colorAttribute = fx_samplerCUBE_common.Type.GetAttributeInfo("border_color");
            fx_samplerCUBE_common.mipmap_maxlevelAttribute = fx_samplerCUBE_common.Type.GetAttributeInfo("mipmap_maxlevel");
            fx_samplerCUBE_common.mipmap_biasAttribute = fx_samplerCUBE_common.Type.GetAttributeInfo("mipmap_bias");
            fx_samplerCUBE_common.extraChild = fx_samplerCUBE_common.Type.GetChildInfo("extra");

            fx_samplerRECT_common.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "fx_samplerRECT_common");
            fx_samplerRECT_common.sourceAttribute = fx_samplerRECT_common.Type.GetAttributeInfo("source");
            fx_samplerRECT_common.wrap_sAttribute = fx_samplerRECT_common.Type.GetAttributeInfo("wrap_s");
            fx_samplerRECT_common.wrap_tAttribute = fx_samplerRECT_common.Type.GetAttributeInfo("wrap_t");
            fx_samplerRECT_common.minfilterAttribute = fx_samplerRECT_common.Type.GetAttributeInfo("minfilter");
            fx_samplerRECT_common.magfilterAttribute = fx_samplerRECT_common.Type.GetAttributeInfo("magfilter");
            fx_samplerRECT_common.mipfilterAttribute = fx_samplerRECT_common.Type.GetAttributeInfo("mipfilter");
            fx_samplerRECT_common.border_colorAttribute = fx_samplerRECT_common.Type.GetAttributeInfo("border_color");
            fx_samplerRECT_common.mipmap_maxlevelAttribute = fx_samplerRECT_common.Type.GetAttributeInfo("mipmap_maxlevel");
            fx_samplerRECT_common.mipmap_biasAttribute = fx_samplerRECT_common.Type.GetAttributeInfo("mipmap_bias");
            fx_samplerRECT_common.extraChild = fx_samplerRECT_common.Type.GetChildInfo("extra");

            fx_samplerDEPTH_common.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "fx_samplerDEPTH_common");
            fx_samplerDEPTH_common.sourceAttribute = fx_samplerDEPTH_common.Type.GetAttributeInfo("source");
            fx_samplerDEPTH_common.wrap_sAttribute = fx_samplerDEPTH_common.Type.GetAttributeInfo("wrap_s");
            fx_samplerDEPTH_common.wrap_tAttribute = fx_samplerDEPTH_common.Type.GetAttributeInfo("wrap_t");
            fx_samplerDEPTH_common.minfilterAttribute = fx_samplerDEPTH_common.Type.GetAttributeInfo("minfilter");
            fx_samplerDEPTH_common.magfilterAttribute = fx_samplerDEPTH_common.Type.GetAttributeInfo("magfilter");
            fx_samplerDEPTH_common.extraChild = fx_samplerDEPTH_common.Type.GetChildInfo("extra");

            profile_CG.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "profile_CG");
            profile_CG.idAttribute = profile_CG.Type.GetAttributeInfo("id");
            profile_CG.platformAttribute = profile_CG.Type.GetAttributeInfo("platform");
            profile_CG.assetChild = profile_CG.Type.GetChildInfo("asset");
            profile_CG.codeChild = profile_CG.Type.GetChildInfo("code");
            profile_CG.includeChild = profile_CG.Type.GetChildInfo("include");
            profile_CG.imageChild = profile_CG.Type.GetChildInfo("image");
            profile_CG.newparamChild = profile_CG.Type.GetChildInfo("newparam");
            profile_CG.techniqueChild = profile_CG.Type.GetChildInfo("technique");
            profile_CG.extraChild = profile_CG.Type.GetChildInfo("extra");

            fx_code_profile.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "fx_code_profile");
            fx_code_profile.Attribute = fx_code_profile.Type.GetAttributeInfo("");
            fx_code_profile.sidAttribute = fx_code_profile.Type.GetAttributeInfo("sid");

            fx_include_common.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "fx_include_common");
            fx_include_common.sidAttribute = fx_include_common.Type.GetAttributeInfo("sid");
            fx_include_common.urlAttribute = fx_include_common.Type.GetAttributeInfo("url");

            cg_newparam.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "cg_newparam");
            cg_newparam.semanticAttribute = cg_newparam.Type.GetAttributeInfo("semantic");
            cg_newparam.modifierAttribute = cg_newparam.Type.GetAttributeInfo("modifier");
            cg_newparam.boolAttribute = cg_newparam.Type.GetAttributeInfo("bool");
            cg_newparam.bool1Attribute = cg_newparam.Type.GetAttributeInfo("bool1");
            cg_newparam.bool2Attribute = cg_newparam.Type.GetAttributeInfo("bool2");
            cg_newparam.bool3Attribute = cg_newparam.Type.GetAttributeInfo("bool3");
            cg_newparam.bool4Attribute = cg_newparam.Type.GetAttributeInfo("bool4");
            cg_newparam.bool1x1Attribute = cg_newparam.Type.GetAttributeInfo("bool1x1");
            cg_newparam.bool1x2Attribute = cg_newparam.Type.GetAttributeInfo("bool1x2");
            cg_newparam.bool1x3Attribute = cg_newparam.Type.GetAttributeInfo("bool1x3");
            cg_newparam.bool1x4Attribute = cg_newparam.Type.GetAttributeInfo("bool1x4");
            cg_newparam.bool2x1Attribute = cg_newparam.Type.GetAttributeInfo("bool2x1");
            cg_newparam.bool2x2Attribute = cg_newparam.Type.GetAttributeInfo("bool2x2");
            cg_newparam.bool2x3Attribute = cg_newparam.Type.GetAttributeInfo("bool2x3");
            cg_newparam.bool2x4Attribute = cg_newparam.Type.GetAttributeInfo("bool2x4");
            cg_newparam.bool3x1Attribute = cg_newparam.Type.GetAttributeInfo("bool3x1");
            cg_newparam.bool3x2Attribute = cg_newparam.Type.GetAttributeInfo("bool3x2");
            cg_newparam.bool3x3Attribute = cg_newparam.Type.GetAttributeInfo("bool3x3");
            cg_newparam.bool3x4Attribute = cg_newparam.Type.GetAttributeInfo("bool3x4");
            cg_newparam.bool4x1Attribute = cg_newparam.Type.GetAttributeInfo("bool4x1");
            cg_newparam.bool4x2Attribute = cg_newparam.Type.GetAttributeInfo("bool4x2");
            cg_newparam.bool4x3Attribute = cg_newparam.Type.GetAttributeInfo("bool4x3");
            cg_newparam.bool4x4Attribute = cg_newparam.Type.GetAttributeInfo("bool4x4");
            cg_newparam.floatAttribute = cg_newparam.Type.GetAttributeInfo("float");
            cg_newparam.float1Attribute = cg_newparam.Type.GetAttributeInfo("float1");
            cg_newparam.float2Attribute = cg_newparam.Type.GetAttributeInfo("float2");
            cg_newparam.float3Attribute = cg_newparam.Type.GetAttributeInfo("float3");
            cg_newparam.float4Attribute = cg_newparam.Type.GetAttributeInfo("float4");
            cg_newparam.float1x1Attribute = cg_newparam.Type.GetAttributeInfo("float1x1");
            cg_newparam.float1x2Attribute = cg_newparam.Type.GetAttributeInfo("float1x2");
            cg_newparam.float1x3Attribute = cg_newparam.Type.GetAttributeInfo("float1x3");
            cg_newparam.float1x4Attribute = cg_newparam.Type.GetAttributeInfo("float1x4");
            cg_newparam.float2x1Attribute = cg_newparam.Type.GetAttributeInfo("float2x1");
            cg_newparam.float2x2Attribute = cg_newparam.Type.GetAttributeInfo("float2x2");
            cg_newparam.float2x3Attribute = cg_newparam.Type.GetAttributeInfo("float2x3");
            cg_newparam.float2x4Attribute = cg_newparam.Type.GetAttributeInfo("float2x4");
            cg_newparam.float3x1Attribute = cg_newparam.Type.GetAttributeInfo("float3x1");
            cg_newparam.float3x2Attribute = cg_newparam.Type.GetAttributeInfo("float3x2");
            cg_newparam.float3x3Attribute = cg_newparam.Type.GetAttributeInfo("float3x3");
            cg_newparam.float3x4Attribute = cg_newparam.Type.GetAttributeInfo("float3x4");
            cg_newparam.float4x1Attribute = cg_newparam.Type.GetAttributeInfo("float4x1");
            cg_newparam.float4x2Attribute = cg_newparam.Type.GetAttributeInfo("float4x2");
            cg_newparam.float4x3Attribute = cg_newparam.Type.GetAttributeInfo("float4x3");
            cg_newparam.float4x4Attribute = cg_newparam.Type.GetAttributeInfo("float4x4");
            cg_newparam.intAttribute = cg_newparam.Type.GetAttributeInfo("int");
            cg_newparam.int1Attribute = cg_newparam.Type.GetAttributeInfo("int1");
            cg_newparam.int2Attribute = cg_newparam.Type.GetAttributeInfo("int2");
            cg_newparam.int3Attribute = cg_newparam.Type.GetAttributeInfo("int3");
            cg_newparam.int4Attribute = cg_newparam.Type.GetAttributeInfo("int4");
            cg_newparam.int1x1Attribute = cg_newparam.Type.GetAttributeInfo("int1x1");
            cg_newparam.int1x2Attribute = cg_newparam.Type.GetAttributeInfo("int1x2");
            cg_newparam.int1x3Attribute = cg_newparam.Type.GetAttributeInfo("int1x3");
            cg_newparam.int1x4Attribute = cg_newparam.Type.GetAttributeInfo("int1x4");
            cg_newparam.int2x1Attribute = cg_newparam.Type.GetAttributeInfo("int2x1");
            cg_newparam.int2x2Attribute = cg_newparam.Type.GetAttributeInfo("int2x2");
            cg_newparam.int2x3Attribute = cg_newparam.Type.GetAttributeInfo("int2x3");
            cg_newparam.int2x4Attribute = cg_newparam.Type.GetAttributeInfo("int2x4");
            cg_newparam.int3x1Attribute = cg_newparam.Type.GetAttributeInfo("int3x1");
            cg_newparam.int3x2Attribute = cg_newparam.Type.GetAttributeInfo("int3x2");
            cg_newparam.int3x3Attribute = cg_newparam.Type.GetAttributeInfo("int3x3");
            cg_newparam.int3x4Attribute = cg_newparam.Type.GetAttributeInfo("int3x4");
            cg_newparam.int4x1Attribute = cg_newparam.Type.GetAttributeInfo("int4x1");
            cg_newparam.int4x2Attribute = cg_newparam.Type.GetAttributeInfo("int4x2");
            cg_newparam.int4x3Attribute = cg_newparam.Type.GetAttributeInfo("int4x3");
            cg_newparam.int4x4Attribute = cg_newparam.Type.GetAttributeInfo("int4x4");
            cg_newparam.halfAttribute = cg_newparam.Type.GetAttributeInfo("half");
            cg_newparam.half1Attribute = cg_newparam.Type.GetAttributeInfo("half1");
            cg_newparam.half2Attribute = cg_newparam.Type.GetAttributeInfo("half2");
            cg_newparam.half3Attribute = cg_newparam.Type.GetAttributeInfo("half3");
            cg_newparam.half4Attribute = cg_newparam.Type.GetAttributeInfo("half4");
            cg_newparam.half1x1Attribute = cg_newparam.Type.GetAttributeInfo("half1x1");
            cg_newparam.half1x2Attribute = cg_newparam.Type.GetAttributeInfo("half1x2");
            cg_newparam.half1x3Attribute = cg_newparam.Type.GetAttributeInfo("half1x3");
            cg_newparam.half1x4Attribute = cg_newparam.Type.GetAttributeInfo("half1x4");
            cg_newparam.half2x1Attribute = cg_newparam.Type.GetAttributeInfo("half2x1");
            cg_newparam.half2x2Attribute = cg_newparam.Type.GetAttributeInfo("half2x2");
            cg_newparam.half2x3Attribute = cg_newparam.Type.GetAttributeInfo("half2x3");
            cg_newparam.half2x4Attribute = cg_newparam.Type.GetAttributeInfo("half2x4");
            cg_newparam.half3x1Attribute = cg_newparam.Type.GetAttributeInfo("half3x1");
            cg_newparam.half3x2Attribute = cg_newparam.Type.GetAttributeInfo("half3x2");
            cg_newparam.half3x3Attribute = cg_newparam.Type.GetAttributeInfo("half3x3");
            cg_newparam.half3x4Attribute = cg_newparam.Type.GetAttributeInfo("half3x4");
            cg_newparam.half4x1Attribute = cg_newparam.Type.GetAttributeInfo("half4x1");
            cg_newparam.half4x2Attribute = cg_newparam.Type.GetAttributeInfo("half4x2");
            cg_newparam.half4x3Attribute = cg_newparam.Type.GetAttributeInfo("half4x3");
            cg_newparam.half4x4Attribute = cg_newparam.Type.GetAttributeInfo("half4x4");
            cg_newparam.fixedAttribute = cg_newparam.Type.GetAttributeInfo("fixed");
            cg_newparam.fixed1Attribute = cg_newparam.Type.GetAttributeInfo("fixed1");
            cg_newparam.fixed2Attribute = cg_newparam.Type.GetAttributeInfo("fixed2");
            cg_newparam.fixed3Attribute = cg_newparam.Type.GetAttributeInfo("fixed3");
            cg_newparam.fixed4Attribute = cg_newparam.Type.GetAttributeInfo("fixed4");
            cg_newparam.fixed1x1Attribute = cg_newparam.Type.GetAttributeInfo("fixed1x1");
            cg_newparam.fixed1x2Attribute = cg_newparam.Type.GetAttributeInfo("fixed1x2");
            cg_newparam.fixed1x3Attribute = cg_newparam.Type.GetAttributeInfo("fixed1x3");
            cg_newparam.fixed1x4Attribute = cg_newparam.Type.GetAttributeInfo("fixed1x4");
            cg_newparam.fixed2x1Attribute = cg_newparam.Type.GetAttributeInfo("fixed2x1");
            cg_newparam.fixed2x2Attribute = cg_newparam.Type.GetAttributeInfo("fixed2x2");
            cg_newparam.fixed2x3Attribute = cg_newparam.Type.GetAttributeInfo("fixed2x3");
            cg_newparam.fixed2x4Attribute = cg_newparam.Type.GetAttributeInfo("fixed2x4");
            cg_newparam.fixed3x1Attribute = cg_newparam.Type.GetAttributeInfo("fixed3x1");
            cg_newparam.fixed3x2Attribute = cg_newparam.Type.GetAttributeInfo("fixed3x2");
            cg_newparam.fixed3x3Attribute = cg_newparam.Type.GetAttributeInfo("fixed3x3");
            cg_newparam.fixed3x4Attribute = cg_newparam.Type.GetAttributeInfo("fixed3x4");
            cg_newparam.fixed4x1Attribute = cg_newparam.Type.GetAttributeInfo("fixed4x1");
            cg_newparam.fixed4x2Attribute = cg_newparam.Type.GetAttributeInfo("fixed4x2");
            cg_newparam.fixed4x3Attribute = cg_newparam.Type.GetAttributeInfo("fixed4x3");
            cg_newparam.fixed4x4Attribute = cg_newparam.Type.GetAttributeInfo("fixed4x4");
            cg_newparam.stringAttribute = cg_newparam.Type.GetAttributeInfo("string");
            cg_newparam.enumAttribute = cg_newparam.Type.GetAttributeInfo("enum");
            cg_newparam.sidAttribute = cg_newparam.Type.GetAttributeInfo("sid");
            cg_newparam.annotateChild = cg_newparam.Type.GetChildInfo("annotate");
            cg_newparam.surfaceChild = cg_newparam.Type.GetChildInfo("surface");
            cg_newparam.sampler1DChild = cg_newparam.Type.GetChildInfo("sampler1D");
            cg_newparam.sampler2DChild = cg_newparam.Type.GetChildInfo("sampler2D");
            cg_newparam.sampler3DChild = cg_newparam.Type.GetChildInfo("sampler3D");
            cg_newparam.samplerRECTChild = cg_newparam.Type.GetChildInfo("samplerRECT");
            cg_newparam.samplerCUBEChild = cg_newparam.Type.GetChildInfo("samplerCUBE");
            cg_newparam.samplerDEPTHChild = cg_newparam.Type.GetChildInfo("samplerDEPTH");
            cg_newparam.usertypeChild = cg_newparam.Type.GetChildInfo("usertype");
            cg_newparam.arrayChild = cg_newparam.Type.GetChildInfo("array");

            cg_surface_type.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "cg_surface_type");
            cg_surface_type.formatAttribute = cg_surface_type.Type.GetAttributeInfo("format");
            cg_surface_type.sizeAttribute = cg_surface_type.Type.GetAttributeInfo("size");
            cg_surface_type.viewport_ratioAttribute = cg_surface_type.Type.GetAttributeInfo("viewport_ratio");
            cg_surface_type.mip_levelsAttribute = cg_surface_type.Type.GetAttributeInfo("mip_levels");
            cg_surface_type.mipmap_generateAttribute = cg_surface_type.Type.GetAttributeInfo("mipmap_generate");
            cg_surface_type.typeAttribute = cg_surface_type.Type.GetAttributeInfo("type");
            cg_surface_type.init_as_nullChild = cg_surface_type.Type.GetChildInfo("init_as_null");
            cg_surface_type.init_as_targetChild = cg_surface_type.Type.GetChildInfo("init_as_target");
            cg_surface_type.init_cubeChild = cg_surface_type.Type.GetChildInfo("init_cube");
            cg_surface_type.init_volumeChild = cg_surface_type.Type.GetChildInfo("init_volume");
            cg_surface_type.init_planarChild = cg_surface_type.Type.GetChildInfo("init_planar");
            cg_surface_type.init_fromChild = cg_surface_type.Type.GetChildInfo("init_from");
            cg_surface_type.format_hintChild = cg_surface_type.Type.GetChildInfo("format_hint");
            cg_surface_type.extraChild = cg_surface_type.Type.GetChildInfo("extra");
            cg_surface_type.generatorChild = cg_surface_type.Type.GetChildInfo("generator");

            cg_surface_type_generator.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "cg_surface_type_generator");
            cg_surface_type_generator.annotateChild = cg_surface_type_generator.Type.GetChildInfo("annotate");
            cg_surface_type_generator.codeChild = cg_surface_type_generator.Type.GetChildInfo("code");
            cg_surface_type_generator.includeChild = cg_surface_type_generator.Type.GetChildInfo("include");
            cg_surface_type_generator.nameChild = cg_surface_type_generator.Type.GetChildInfo("name");
            cg_surface_type_generator.setparamChild = cg_surface_type_generator.Type.GetChildInfo("setparam");

            generator_name.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "generator_name");
            generator_name.Attribute = generator_name.Type.GetAttributeInfo("");
            generator_name.sourceAttribute = generator_name.Type.GetAttributeInfo("source");

            cg_setparam_simple.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "cg_setparam_simple");
            cg_setparam_simple.boolAttribute = cg_setparam_simple.Type.GetAttributeInfo("bool");
            cg_setparam_simple.bool1Attribute = cg_setparam_simple.Type.GetAttributeInfo("bool1");
            cg_setparam_simple.bool2Attribute = cg_setparam_simple.Type.GetAttributeInfo("bool2");
            cg_setparam_simple.bool3Attribute = cg_setparam_simple.Type.GetAttributeInfo("bool3");
            cg_setparam_simple.bool4Attribute = cg_setparam_simple.Type.GetAttributeInfo("bool4");
            cg_setparam_simple.bool1x1Attribute = cg_setparam_simple.Type.GetAttributeInfo("bool1x1");
            cg_setparam_simple.bool1x2Attribute = cg_setparam_simple.Type.GetAttributeInfo("bool1x2");
            cg_setparam_simple.bool1x3Attribute = cg_setparam_simple.Type.GetAttributeInfo("bool1x3");
            cg_setparam_simple.bool1x4Attribute = cg_setparam_simple.Type.GetAttributeInfo("bool1x4");
            cg_setparam_simple.bool2x1Attribute = cg_setparam_simple.Type.GetAttributeInfo("bool2x1");
            cg_setparam_simple.bool2x2Attribute = cg_setparam_simple.Type.GetAttributeInfo("bool2x2");
            cg_setparam_simple.bool2x3Attribute = cg_setparam_simple.Type.GetAttributeInfo("bool2x3");
            cg_setparam_simple.bool2x4Attribute = cg_setparam_simple.Type.GetAttributeInfo("bool2x4");
            cg_setparam_simple.bool3x1Attribute = cg_setparam_simple.Type.GetAttributeInfo("bool3x1");
            cg_setparam_simple.bool3x2Attribute = cg_setparam_simple.Type.GetAttributeInfo("bool3x2");
            cg_setparam_simple.bool3x3Attribute = cg_setparam_simple.Type.GetAttributeInfo("bool3x3");
            cg_setparam_simple.bool3x4Attribute = cg_setparam_simple.Type.GetAttributeInfo("bool3x4");
            cg_setparam_simple.bool4x1Attribute = cg_setparam_simple.Type.GetAttributeInfo("bool4x1");
            cg_setparam_simple.bool4x2Attribute = cg_setparam_simple.Type.GetAttributeInfo("bool4x2");
            cg_setparam_simple.bool4x3Attribute = cg_setparam_simple.Type.GetAttributeInfo("bool4x3");
            cg_setparam_simple.bool4x4Attribute = cg_setparam_simple.Type.GetAttributeInfo("bool4x4");
            cg_setparam_simple.floatAttribute = cg_setparam_simple.Type.GetAttributeInfo("float");
            cg_setparam_simple.float1Attribute = cg_setparam_simple.Type.GetAttributeInfo("float1");
            cg_setparam_simple.float2Attribute = cg_setparam_simple.Type.GetAttributeInfo("float2");
            cg_setparam_simple.float3Attribute = cg_setparam_simple.Type.GetAttributeInfo("float3");
            cg_setparam_simple.float4Attribute = cg_setparam_simple.Type.GetAttributeInfo("float4");
            cg_setparam_simple.float1x1Attribute = cg_setparam_simple.Type.GetAttributeInfo("float1x1");
            cg_setparam_simple.float1x2Attribute = cg_setparam_simple.Type.GetAttributeInfo("float1x2");
            cg_setparam_simple.float1x3Attribute = cg_setparam_simple.Type.GetAttributeInfo("float1x3");
            cg_setparam_simple.float1x4Attribute = cg_setparam_simple.Type.GetAttributeInfo("float1x4");
            cg_setparam_simple.float2x1Attribute = cg_setparam_simple.Type.GetAttributeInfo("float2x1");
            cg_setparam_simple.float2x2Attribute = cg_setparam_simple.Type.GetAttributeInfo("float2x2");
            cg_setparam_simple.float2x3Attribute = cg_setparam_simple.Type.GetAttributeInfo("float2x3");
            cg_setparam_simple.float2x4Attribute = cg_setparam_simple.Type.GetAttributeInfo("float2x4");
            cg_setparam_simple.float3x1Attribute = cg_setparam_simple.Type.GetAttributeInfo("float3x1");
            cg_setparam_simple.float3x2Attribute = cg_setparam_simple.Type.GetAttributeInfo("float3x2");
            cg_setparam_simple.float3x3Attribute = cg_setparam_simple.Type.GetAttributeInfo("float3x3");
            cg_setparam_simple.float3x4Attribute = cg_setparam_simple.Type.GetAttributeInfo("float3x4");
            cg_setparam_simple.float4x1Attribute = cg_setparam_simple.Type.GetAttributeInfo("float4x1");
            cg_setparam_simple.float4x2Attribute = cg_setparam_simple.Type.GetAttributeInfo("float4x2");
            cg_setparam_simple.float4x3Attribute = cg_setparam_simple.Type.GetAttributeInfo("float4x3");
            cg_setparam_simple.float4x4Attribute = cg_setparam_simple.Type.GetAttributeInfo("float4x4");
            cg_setparam_simple.intAttribute = cg_setparam_simple.Type.GetAttributeInfo("int");
            cg_setparam_simple.int1Attribute = cg_setparam_simple.Type.GetAttributeInfo("int1");
            cg_setparam_simple.int2Attribute = cg_setparam_simple.Type.GetAttributeInfo("int2");
            cg_setparam_simple.int3Attribute = cg_setparam_simple.Type.GetAttributeInfo("int3");
            cg_setparam_simple.int4Attribute = cg_setparam_simple.Type.GetAttributeInfo("int4");
            cg_setparam_simple.int1x1Attribute = cg_setparam_simple.Type.GetAttributeInfo("int1x1");
            cg_setparam_simple.int1x2Attribute = cg_setparam_simple.Type.GetAttributeInfo("int1x2");
            cg_setparam_simple.int1x3Attribute = cg_setparam_simple.Type.GetAttributeInfo("int1x3");
            cg_setparam_simple.int1x4Attribute = cg_setparam_simple.Type.GetAttributeInfo("int1x4");
            cg_setparam_simple.int2x1Attribute = cg_setparam_simple.Type.GetAttributeInfo("int2x1");
            cg_setparam_simple.int2x2Attribute = cg_setparam_simple.Type.GetAttributeInfo("int2x2");
            cg_setparam_simple.int2x3Attribute = cg_setparam_simple.Type.GetAttributeInfo("int2x3");
            cg_setparam_simple.int2x4Attribute = cg_setparam_simple.Type.GetAttributeInfo("int2x4");
            cg_setparam_simple.int3x1Attribute = cg_setparam_simple.Type.GetAttributeInfo("int3x1");
            cg_setparam_simple.int3x2Attribute = cg_setparam_simple.Type.GetAttributeInfo("int3x2");
            cg_setparam_simple.int3x3Attribute = cg_setparam_simple.Type.GetAttributeInfo("int3x3");
            cg_setparam_simple.int3x4Attribute = cg_setparam_simple.Type.GetAttributeInfo("int3x4");
            cg_setparam_simple.int4x1Attribute = cg_setparam_simple.Type.GetAttributeInfo("int4x1");
            cg_setparam_simple.int4x2Attribute = cg_setparam_simple.Type.GetAttributeInfo("int4x2");
            cg_setparam_simple.int4x3Attribute = cg_setparam_simple.Type.GetAttributeInfo("int4x3");
            cg_setparam_simple.int4x4Attribute = cg_setparam_simple.Type.GetAttributeInfo("int4x4");
            cg_setparam_simple.halfAttribute = cg_setparam_simple.Type.GetAttributeInfo("half");
            cg_setparam_simple.half1Attribute = cg_setparam_simple.Type.GetAttributeInfo("half1");
            cg_setparam_simple.half2Attribute = cg_setparam_simple.Type.GetAttributeInfo("half2");
            cg_setparam_simple.half3Attribute = cg_setparam_simple.Type.GetAttributeInfo("half3");
            cg_setparam_simple.half4Attribute = cg_setparam_simple.Type.GetAttributeInfo("half4");
            cg_setparam_simple.half1x1Attribute = cg_setparam_simple.Type.GetAttributeInfo("half1x1");
            cg_setparam_simple.half1x2Attribute = cg_setparam_simple.Type.GetAttributeInfo("half1x2");
            cg_setparam_simple.half1x3Attribute = cg_setparam_simple.Type.GetAttributeInfo("half1x3");
            cg_setparam_simple.half1x4Attribute = cg_setparam_simple.Type.GetAttributeInfo("half1x4");
            cg_setparam_simple.half2x1Attribute = cg_setparam_simple.Type.GetAttributeInfo("half2x1");
            cg_setparam_simple.half2x2Attribute = cg_setparam_simple.Type.GetAttributeInfo("half2x2");
            cg_setparam_simple.half2x3Attribute = cg_setparam_simple.Type.GetAttributeInfo("half2x3");
            cg_setparam_simple.half2x4Attribute = cg_setparam_simple.Type.GetAttributeInfo("half2x4");
            cg_setparam_simple.half3x1Attribute = cg_setparam_simple.Type.GetAttributeInfo("half3x1");
            cg_setparam_simple.half3x2Attribute = cg_setparam_simple.Type.GetAttributeInfo("half3x2");
            cg_setparam_simple.half3x3Attribute = cg_setparam_simple.Type.GetAttributeInfo("half3x3");
            cg_setparam_simple.half3x4Attribute = cg_setparam_simple.Type.GetAttributeInfo("half3x4");
            cg_setparam_simple.half4x1Attribute = cg_setparam_simple.Type.GetAttributeInfo("half4x1");
            cg_setparam_simple.half4x2Attribute = cg_setparam_simple.Type.GetAttributeInfo("half4x2");
            cg_setparam_simple.half4x3Attribute = cg_setparam_simple.Type.GetAttributeInfo("half4x3");
            cg_setparam_simple.half4x4Attribute = cg_setparam_simple.Type.GetAttributeInfo("half4x4");
            cg_setparam_simple.fixedAttribute = cg_setparam_simple.Type.GetAttributeInfo("fixed");
            cg_setparam_simple.fixed1Attribute = cg_setparam_simple.Type.GetAttributeInfo("fixed1");
            cg_setparam_simple.fixed2Attribute = cg_setparam_simple.Type.GetAttributeInfo("fixed2");
            cg_setparam_simple.fixed3Attribute = cg_setparam_simple.Type.GetAttributeInfo("fixed3");
            cg_setparam_simple.fixed4Attribute = cg_setparam_simple.Type.GetAttributeInfo("fixed4");
            cg_setparam_simple.fixed1x1Attribute = cg_setparam_simple.Type.GetAttributeInfo("fixed1x1");
            cg_setparam_simple.fixed1x2Attribute = cg_setparam_simple.Type.GetAttributeInfo("fixed1x2");
            cg_setparam_simple.fixed1x3Attribute = cg_setparam_simple.Type.GetAttributeInfo("fixed1x3");
            cg_setparam_simple.fixed1x4Attribute = cg_setparam_simple.Type.GetAttributeInfo("fixed1x4");
            cg_setparam_simple.fixed2x1Attribute = cg_setparam_simple.Type.GetAttributeInfo("fixed2x1");
            cg_setparam_simple.fixed2x2Attribute = cg_setparam_simple.Type.GetAttributeInfo("fixed2x2");
            cg_setparam_simple.fixed2x3Attribute = cg_setparam_simple.Type.GetAttributeInfo("fixed2x3");
            cg_setparam_simple.fixed2x4Attribute = cg_setparam_simple.Type.GetAttributeInfo("fixed2x4");
            cg_setparam_simple.fixed3x1Attribute = cg_setparam_simple.Type.GetAttributeInfo("fixed3x1");
            cg_setparam_simple.fixed3x2Attribute = cg_setparam_simple.Type.GetAttributeInfo("fixed3x2");
            cg_setparam_simple.fixed3x3Attribute = cg_setparam_simple.Type.GetAttributeInfo("fixed3x3");
            cg_setparam_simple.fixed3x4Attribute = cg_setparam_simple.Type.GetAttributeInfo("fixed3x4");
            cg_setparam_simple.fixed4x1Attribute = cg_setparam_simple.Type.GetAttributeInfo("fixed4x1");
            cg_setparam_simple.fixed4x2Attribute = cg_setparam_simple.Type.GetAttributeInfo("fixed4x2");
            cg_setparam_simple.fixed4x3Attribute = cg_setparam_simple.Type.GetAttributeInfo("fixed4x3");
            cg_setparam_simple.fixed4x4Attribute = cg_setparam_simple.Type.GetAttributeInfo("fixed4x4");
            cg_setparam_simple.stringAttribute = cg_setparam_simple.Type.GetAttributeInfo("string");
            cg_setparam_simple.enumAttribute = cg_setparam_simple.Type.GetAttributeInfo("enum");
            cg_setparam_simple.refAttribute = cg_setparam_simple.Type.GetAttributeInfo("ref");
            cg_setparam_simple.annotateChild = cg_setparam_simple.Type.GetChildInfo("annotate");
            cg_setparam_simple.surfaceChild = cg_setparam_simple.Type.GetChildInfo("surface");
            cg_setparam_simple.sampler1DChild = cg_setparam_simple.Type.GetChildInfo("sampler1D");
            cg_setparam_simple.sampler2DChild = cg_setparam_simple.Type.GetChildInfo("sampler2D");
            cg_setparam_simple.sampler3DChild = cg_setparam_simple.Type.GetChildInfo("sampler3D");
            cg_setparam_simple.samplerRECTChild = cg_setparam_simple.Type.GetChildInfo("samplerRECT");
            cg_setparam_simple.samplerCUBEChild = cg_setparam_simple.Type.GetChildInfo("samplerCUBE");
            cg_setparam_simple.samplerDEPTHChild = cg_setparam_simple.Type.GetChildInfo("samplerDEPTH");

            cg_sampler1D.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "cg_sampler1D");
            cg_sampler1D.sourceAttribute = cg_sampler1D.Type.GetAttributeInfo("source");
            cg_sampler1D.wrap_sAttribute = cg_sampler1D.Type.GetAttributeInfo("wrap_s");
            cg_sampler1D.minfilterAttribute = cg_sampler1D.Type.GetAttributeInfo("minfilter");
            cg_sampler1D.magfilterAttribute = cg_sampler1D.Type.GetAttributeInfo("magfilter");
            cg_sampler1D.mipfilterAttribute = cg_sampler1D.Type.GetAttributeInfo("mipfilter");
            cg_sampler1D.border_colorAttribute = cg_sampler1D.Type.GetAttributeInfo("border_color");
            cg_sampler1D.mipmap_maxlevelAttribute = cg_sampler1D.Type.GetAttributeInfo("mipmap_maxlevel");
            cg_sampler1D.mipmap_biasAttribute = cg_sampler1D.Type.GetAttributeInfo("mipmap_bias");
            cg_sampler1D.extraChild = cg_sampler1D.Type.GetChildInfo("extra");

            cg_sampler2D.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "cg_sampler2D");
            cg_sampler2D.sourceAttribute = cg_sampler2D.Type.GetAttributeInfo("source");
            cg_sampler2D.wrap_sAttribute = cg_sampler2D.Type.GetAttributeInfo("wrap_s");
            cg_sampler2D.wrap_tAttribute = cg_sampler2D.Type.GetAttributeInfo("wrap_t");
            cg_sampler2D.minfilterAttribute = cg_sampler2D.Type.GetAttributeInfo("minfilter");
            cg_sampler2D.magfilterAttribute = cg_sampler2D.Type.GetAttributeInfo("magfilter");
            cg_sampler2D.mipfilterAttribute = cg_sampler2D.Type.GetAttributeInfo("mipfilter");
            cg_sampler2D.border_colorAttribute = cg_sampler2D.Type.GetAttributeInfo("border_color");
            cg_sampler2D.mipmap_maxlevelAttribute = cg_sampler2D.Type.GetAttributeInfo("mipmap_maxlevel");
            cg_sampler2D.mipmap_biasAttribute = cg_sampler2D.Type.GetAttributeInfo("mipmap_bias");
            cg_sampler2D.extraChild = cg_sampler2D.Type.GetChildInfo("extra");

            cg_sampler3D.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "cg_sampler3D");
            cg_sampler3D.sourceAttribute = cg_sampler3D.Type.GetAttributeInfo("source");
            cg_sampler3D.wrap_sAttribute = cg_sampler3D.Type.GetAttributeInfo("wrap_s");
            cg_sampler3D.wrap_tAttribute = cg_sampler3D.Type.GetAttributeInfo("wrap_t");
            cg_sampler3D.wrap_pAttribute = cg_sampler3D.Type.GetAttributeInfo("wrap_p");
            cg_sampler3D.minfilterAttribute = cg_sampler3D.Type.GetAttributeInfo("minfilter");
            cg_sampler3D.magfilterAttribute = cg_sampler3D.Type.GetAttributeInfo("magfilter");
            cg_sampler3D.mipfilterAttribute = cg_sampler3D.Type.GetAttributeInfo("mipfilter");
            cg_sampler3D.border_colorAttribute = cg_sampler3D.Type.GetAttributeInfo("border_color");
            cg_sampler3D.mipmap_maxlevelAttribute = cg_sampler3D.Type.GetAttributeInfo("mipmap_maxlevel");
            cg_sampler3D.mipmap_biasAttribute = cg_sampler3D.Type.GetAttributeInfo("mipmap_bias");
            cg_sampler3D.extraChild = cg_sampler3D.Type.GetChildInfo("extra");

            cg_samplerRECT.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "cg_samplerRECT");
            cg_samplerRECT.sourceAttribute = cg_samplerRECT.Type.GetAttributeInfo("source");
            cg_samplerRECT.wrap_sAttribute = cg_samplerRECT.Type.GetAttributeInfo("wrap_s");
            cg_samplerRECT.wrap_tAttribute = cg_samplerRECT.Type.GetAttributeInfo("wrap_t");
            cg_samplerRECT.minfilterAttribute = cg_samplerRECT.Type.GetAttributeInfo("minfilter");
            cg_samplerRECT.magfilterAttribute = cg_samplerRECT.Type.GetAttributeInfo("magfilter");
            cg_samplerRECT.mipfilterAttribute = cg_samplerRECT.Type.GetAttributeInfo("mipfilter");
            cg_samplerRECT.border_colorAttribute = cg_samplerRECT.Type.GetAttributeInfo("border_color");
            cg_samplerRECT.mipmap_maxlevelAttribute = cg_samplerRECT.Type.GetAttributeInfo("mipmap_maxlevel");
            cg_samplerRECT.mipmap_biasAttribute = cg_samplerRECT.Type.GetAttributeInfo("mipmap_bias");
            cg_samplerRECT.extraChild = cg_samplerRECT.Type.GetChildInfo("extra");

            cg_samplerCUBE.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "cg_samplerCUBE");
            cg_samplerCUBE.sourceAttribute = cg_samplerCUBE.Type.GetAttributeInfo("source");
            cg_samplerCUBE.wrap_sAttribute = cg_samplerCUBE.Type.GetAttributeInfo("wrap_s");
            cg_samplerCUBE.wrap_tAttribute = cg_samplerCUBE.Type.GetAttributeInfo("wrap_t");
            cg_samplerCUBE.wrap_pAttribute = cg_samplerCUBE.Type.GetAttributeInfo("wrap_p");
            cg_samplerCUBE.minfilterAttribute = cg_samplerCUBE.Type.GetAttributeInfo("minfilter");
            cg_samplerCUBE.magfilterAttribute = cg_samplerCUBE.Type.GetAttributeInfo("magfilter");
            cg_samplerCUBE.mipfilterAttribute = cg_samplerCUBE.Type.GetAttributeInfo("mipfilter");
            cg_samplerCUBE.border_colorAttribute = cg_samplerCUBE.Type.GetAttributeInfo("border_color");
            cg_samplerCUBE.mipmap_maxlevelAttribute = cg_samplerCUBE.Type.GetAttributeInfo("mipmap_maxlevel");
            cg_samplerCUBE.mipmap_biasAttribute = cg_samplerCUBE.Type.GetAttributeInfo("mipmap_bias");
            cg_samplerCUBE.extraChild = cg_samplerCUBE.Type.GetChildInfo("extra");

            cg_samplerDEPTH.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "cg_samplerDEPTH");
            cg_samplerDEPTH.sourceAttribute = cg_samplerDEPTH.Type.GetAttributeInfo("source");
            cg_samplerDEPTH.wrap_sAttribute = cg_samplerDEPTH.Type.GetAttributeInfo("wrap_s");
            cg_samplerDEPTH.wrap_tAttribute = cg_samplerDEPTH.Type.GetAttributeInfo("wrap_t");
            cg_samplerDEPTH.minfilterAttribute = cg_samplerDEPTH.Type.GetAttributeInfo("minfilter");
            cg_samplerDEPTH.magfilterAttribute = cg_samplerDEPTH.Type.GetAttributeInfo("magfilter");
            cg_samplerDEPTH.extraChild = cg_samplerDEPTH.Type.GetChildInfo("extra");

            cg_setuser_type.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "cg_setuser_type");
            cg_setuser_type.boolAttribute = cg_setuser_type.Type.GetAttributeInfo("bool");
            cg_setuser_type.bool1Attribute = cg_setuser_type.Type.GetAttributeInfo("bool1");
            cg_setuser_type.bool2Attribute = cg_setuser_type.Type.GetAttributeInfo("bool2");
            cg_setuser_type.bool3Attribute = cg_setuser_type.Type.GetAttributeInfo("bool3");
            cg_setuser_type.bool4Attribute = cg_setuser_type.Type.GetAttributeInfo("bool4");
            cg_setuser_type.bool1x1Attribute = cg_setuser_type.Type.GetAttributeInfo("bool1x1");
            cg_setuser_type.bool1x2Attribute = cg_setuser_type.Type.GetAttributeInfo("bool1x2");
            cg_setuser_type.bool1x3Attribute = cg_setuser_type.Type.GetAttributeInfo("bool1x3");
            cg_setuser_type.bool1x4Attribute = cg_setuser_type.Type.GetAttributeInfo("bool1x4");
            cg_setuser_type.bool2x1Attribute = cg_setuser_type.Type.GetAttributeInfo("bool2x1");
            cg_setuser_type.bool2x2Attribute = cg_setuser_type.Type.GetAttributeInfo("bool2x2");
            cg_setuser_type.bool2x3Attribute = cg_setuser_type.Type.GetAttributeInfo("bool2x3");
            cg_setuser_type.bool2x4Attribute = cg_setuser_type.Type.GetAttributeInfo("bool2x4");
            cg_setuser_type.bool3x1Attribute = cg_setuser_type.Type.GetAttributeInfo("bool3x1");
            cg_setuser_type.bool3x2Attribute = cg_setuser_type.Type.GetAttributeInfo("bool3x2");
            cg_setuser_type.bool3x3Attribute = cg_setuser_type.Type.GetAttributeInfo("bool3x3");
            cg_setuser_type.bool3x4Attribute = cg_setuser_type.Type.GetAttributeInfo("bool3x4");
            cg_setuser_type.bool4x1Attribute = cg_setuser_type.Type.GetAttributeInfo("bool4x1");
            cg_setuser_type.bool4x2Attribute = cg_setuser_type.Type.GetAttributeInfo("bool4x2");
            cg_setuser_type.bool4x3Attribute = cg_setuser_type.Type.GetAttributeInfo("bool4x3");
            cg_setuser_type.bool4x4Attribute = cg_setuser_type.Type.GetAttributeInfo("bool4x4");
            cg_setuser_type.floatAttribute = cg_setuser_type.Type.GetAttributeInfo("float");
            cg_setuser_type.float1Attribute = cg_setuser_type.Type.GetAttributeInfo("float1");
            cg_setuser_type.float2Attribute = cg_setuser_type.Type.GetAttributeInfo("float2");
            cg_setuser_type.float3Attribute = cg_setuser_type.Type.GetAttributeInfo("float3");
            cg_setuser_type.float4Attribute = cg_setuser_type.Type.GetAttributeInfo("float4");
            cg_setuser_type.float1x1Attribute = cg_setuser_type.Type.GetAttributeInfo("float1x1");
            cg_setuser_type.float1x2Attribute = cg_setuser_type.Type.GetAttributeInfo("float1x2");
            cg_setuser_type.float1x3Attribute = cg_setuser_type.Type.GetAttributeInfo("float1x3");
            cg_setuser_type.float1x4Attribute = cg_setuser_type.Type.GetAttributeInfo("float1x4");
            cg_setuser_type.float2x1Attribute = cg_setuser_type.Type.GetAttributeInfo("float2x1");
            cg_setuser_type.float2x2Attribute = cg_setuser_type.Type.GetAttributeInfo("float2x2");
            cg_setuser_type.float2x3Attribute = cg_setuser_type.Type.GetAttributeInfo("float2x3");
            cg_setuser_type.float2x4Attribute = cg_setuser_type.Type.GetAttributeInfo("float2x4");
            cg_setuser_type.float3x1Attribute = cg_setuser_type.Type.GetAttributeInfo("float3x1");
            cg_setuser_type.float3x2Attribute = cg_setuser_type.Type.GetAttributeInfo("float3x2");
            cg_setuser_type.float3x3Attribute = cg_setuser_type.Type.GetAttributeInfo("float3x3");
            cg_setuser_type.float3x4Attribute = cg_setuser_type.Type.GetAttributeInfo("float3x4");
            cg_setuser_type.float4x1Attribute = cg_setuser_type.Type.GetAttributeInfo("float4x1");
            cg_setuser_type.float4x2Attribute = cg_setuser_type.Type.GetAttributeInfo("float4x2");
            cg_setuser_type.float4x3Attribute = cg_setuser_type.Type.GetAttributeInfo("float4x3");
            cg_setuser_type.float4x4Attribute = cg_setuser_type.Type.GetAttributeInfo("float4x4");
            cg_setuser_type.intAttribute = cg_setuser_type.Type.GetAttributeInfo("int");
            cg_setuser_type.int1Attribute = cg_setuser_type.Type.GetAttributeInfo("int1");
            cg_setuser_type.int2Attribute = cg_setuser_type.Type.GetAttributeInfo("int2");
            cg_setuser_type.int3Attribute = cg_setuser_type.Type.GetAttributeInfo("int3");
            cg_setuser_type.int4Attribute = cg_setuser_type.Type.GetAttributeInfo("int4");
            cg_setuser_type.int1x1Attribute = cg_setuser_type.Type.GetAttributeInfo("int1x1");
            cg_setuser_type.int1x2Attribute = cg_setuser_type.Type.GetAttributeInfo("int1x2");
            cg_setuser_type.int1x3Attribute = cg_setuser_type.Type.GetAttributeInfo("int1x3");
            cg_setuser_type.int1x4Attribute = cg_setuser_type.Type.GetAttributeInfo("int1x4");
            cg_setuser_type.int2x1Attribute = cg_setuser_type.Type.GetAttributeInfo("int2x1");
            cg_setuser_type.int2x2Attribute = cg_setuser_type.Type.GetAttributeInfo("int2x2");
            cg_setuser_type.int2x3Attribute = cg_setuser_type.Type.GetAttributeInfo("int2x3");
            cg_setuser_type.int2x4Attribute = cg_setuser_type.Type.GetAttributeInfo("int2x4");
            cg_setuser_type.int3x1Attribute = cg_setuser_type.Type.GetAttributeInfo("int3x1");
            cg_setuser_type.int3x2Attribute = cg_setuser_type.Type.GetAttributeInfo("int3x2");
            cg_setuser_type.int3x3Attribute = cg_setuser_type.Type.GetAttributeInfo("int3x3");
            cg_setuser_type.int3x4Attribute = cg_setuser_type.Type.GetAttributeInfo("int3x4");
            cg_setuser_type.int4x1Attribute = cg_setuser_type.Type.GetAttributeInfo("int4x1");
            cg_setuser_type.int4x2Attribute = cg_setuser_type.Type.GetAttributeInfo("int4x2");
            cg_setuser_type.int4x3Attribute = cg_setuser_type.Type.GetAttributeInfo("int4x3");
            cg_setuser_type.int4x4Attribute = cg_setuser_type.Type.GetAttributeInfo("int4x4");
            cg_setuser_type.halfAttribute = cg_setuser_type.Type.GetAttributeInfo("half");
            cg_setuser_type.half1Attribute = cg_setuser_type.Type.GetAttributeInfo("half1");
            cg_setuser_type.half2Attribute = cg_setuser_type.Type.GetAttributeInfo("half2");
            cg_setuser_type.half3Attribute = cg_setuser_type.Type.GetAttributeInfo("half3");
            cg_setuser_type.half4Attribute = cg_setuser_type.Type.GetAttributeInfo("half4");
            cg_setuser_type.half1x1Attribute = cg_setuser_type.Type.GetAttributeInfo("half1x1");
            cg_setuser_type.half1x2Attribute = cg_setuser_type.Type.GetAttributeInfo("half1x2");
            cg_setuser_type.half1x3Attribute = cg_setuser_type.Type.GetAttributeInfo("half1x3");
            cg_setuser_type.half1x4Attribute = cg_setuser_type.Type.GetAttributeInfo("half1x4");
            cg_setuser_type.half2x1Attribute = cg_setuser_type.Type.GetAttributeInfo("half2x1");
            cg_setuser_type.half2x2Attribute = cg_setuser_type.Type.GetAttributeInfo("half2x2");
            cg_setuser_type.half2x3Attribute = cg_setuser_type.Type.GetAttributeInfo("half2x3");
            cg_setuser_type.half2x4Attribute = cg_setuser_type.Type.GetAttributeInfo("half2x4");
            cg_setuser_type.half3x1Attribute = cg_setuser_type.Type.GetAttributeInfo("half3x1");
            cg_setuser_type.half3x2Attribute = cg_setuser_type.Type.GetAttributeInfo("half3x2");
            cg_setuser_type.half3x3Attribute = cg_setuser_type.Type.GetAttributeInfo("half3x3");
            cg_setuser_type.half3x4Attribute = cg_setuser_type.Type.GetAttributeInfo("half3x4");
            cg_setuser_type.half4x1Attribute = cg_setuser_type.Type.GetAttributeInfo("half4x1");
            cg_setuser_type.half4x2Attribute = cg_setuser_type.Type.GetAttributeInfo("half4x2");
            cg_setuser_type.half4x3Attribute = cg_setuser_type.Type.GetAttributeInfo("half4x3");
            cg_setuser_type.half4x4Attribute = cg_setuser_type.Type.GetAttributeInfo("half4x4");
            cg_setuser_type.fixedAttribute = cg_setuser_type.Type.GetAttributeInfo("fixed");
            cg_setuser_type.fixed1Attribute = cg_setuser_type.Type.GetAttributeInfo("fixed1");
            cg_setuser_type.fixed2Attribute = cg_setuser_type.Type.GetAttributeInfo("fixed2");
            cg_setuser_type.fixed3Attribute = cg_setuser_type.Type.GetAttributeInfo("fixed3");
            cg_setuser_type.fixed4Attribute = cg_setuser_type.Type.GetAttributeInfo("fixed4");
            cg_setuser_type.fixed1x1Attribute = cg_setuser_type.Type.GetAttributeInfo("fixed1x1");
            cg_setuser_type.fixed1x2Attribute = cg_setuser_type.Type.GetAttributeInfo("fixed1x2");
            cg_setuser_type.fixed1x3Attribute = cg_setuser_type.Type.GetAttributeInfo("fixed1x3");
            cg_setuser_type.fixed1x4Attribute = cg_setuser_type.Type.GetAttributeInfo("fixed1x4");
            cg_setuser_type.fixed2x1Attribute = cg_setuser_type.Type.GetAttributeInfo("fixed2x1");
            cg_setuser_type.fixed2x2Attribute = cg_setuser_type.Type.GetAttributeInfo("fixed2x2");
            cg_setuser_type.fixed2x3Attribute = cg_setuser_type.Type.GetAttributeInfo("fixed2x3");
            cg_setuser_type.fixed2x4Attribute = cg_setuser_type.Type.GetAttributeInfo("fixed2x4");
            cg_setuser_type.fixed3x1Attribute = cg_setuser_type.Type.GetAttributeInfo("fixed3x1");
            cg_setuser_type.fixed3x2Attribute = cg_setuser_type.Type.GetAttributeInfo("fixed3x2");
            cg_setuser_type.fixed3x3Attribute = cg_setuser_type.Type.GetAttributeInfo("fixed3x3");
            cg_setuser_type.fixed3x4Attribute = cg_setuser_type.Type.GetAttributeInfo("fixed3x4");
            cg_setuser_type.fixed4x1Attribute = cg_setuser_type.Type.GetAttributeInfo("fixed4x1");
            cg_setuser_type.fixed4x2Attribute = cg_setuser_type.Type.GetAttributeInfo("fixed4x2");
            cg_setuser_type.fixed4x3Attribute = cg_setuser_type.Type.GetAttributeInfo("fixed4x3");
            cg_setuser_type.fixed4x4Attribute = cg_setuser_type.Type.GetAttributeInfo("fixed4x4");
            cg_setuser_type.stringAttribute = cg_setuser_type.Type.GetAttributeInfo("string");
            cg_setuser_type.enumAttribute = cg_setuser_type.Type.GetAttributeInfo("enum");
            cg_setuser_type.nameAttribute = cg_setuser_type.Type.GetAttributeInfo("name");
            cg_setuser_type.sourceAttribute = cg_setuser_type.Type.GetAttributeInfo("source");
            cg_setuser_type.surfaceChild = cg_setuser_type.Type.GetChildInfo("surface");
            cg_setuser_type.sampler1DChild = cg_setuser_type.Type.GetChildInfo("sampler1D");
            cg_setuser_type.sampler2DChild = cg_setuser_type.Type.GetChildInfo("sampler2D");
            cg_setuser_type.sampler3DChild = cg_setuser_type.Type.GetChildInfo("sampler3D");
            cg_setuser_type.samplerRECTChild = cg_setuser_type.Type.GetChildInfo("samplerRECT");
            cg_setuser_type.samplerCUBEChild = cg_setuser_type.Type.GetChildInfo("samplerCUBE");
            cg_setuser_type.samplerDEPTHChild = cg_setuser_type.Type.GetChildInfo("samplerDEPTH");
            cg_setuser_type.arrayChild = cg_setuser_type.Type.GetChildInfo("array");
            cg_setuser_type.usertypeChild = cg_setuser_type.Type.GetChildInfo("usertype");
            cg_setuser_type.connect_paramChild = cg_setuser_type.Type.GetChildInfo("connect_param");
            cg_setuser_type.setparamChild = cg_setuser_type.Type.GetChildInfo("setparam");

            cg_setarray_type.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "cg_setarray_type");
            cg_setarray_type.boolAttribute = cg_setarray_type.Type.GetAttributeInfo("bool");
            cg_setarray_type.bool1Attribute = cg_setarray_type.Type.GetAttributeInfo("bool1");
            cg_setarray_type.bool2Attribute = cg_setarray_type.Type.GetAttributeInfo("bool2");
            cg_setarray_type.bool3Attribute = cg_setarray_type.Type.GetAttributeInfo("bool3");
            cg_setarray_type.bool4Attribute = cg_setarray_type.Type.GetAttributeInfo("bool4");
            cg_setarray_type.bool1x1Attribute = cg_setarray_type.Type.GetAttributeInfo("bool1x1");
            cg_setarray_type.bool1x2Attribute = cg_setarray_type.Type.GetAttributeInfo("bool1x2");
            cg_setarray_type.bool1x3Attribute = cg_setarray_type.Type.GetAttributeInfo("bool1x3");
            cg_setarray_type.bool1x4Attribute = cg_setarray_type.Type.GetAttributeInfo("bool1x4");
            cg_setarray_type.bool2x1Attribute = cg_setarray_type.Type.GetAttributeInfo("bool2x1");
            cg_setarray_type.bool2x2Attribute = cg_setarray_type.Type.GetAttributeInfo("bool2x2");
            cg_setarray_type.bool2x3Attribute = cg_setarray_type.Type.GetAttributeInfo("bool2x3");
            cg_setarray_type.bool2x4Attribute = cg_setarray_type.Type.GetAttributeInfo("bool2x4");
            cg_setarray_type.bool3x1Attribute = cg_setarray_type.Type.GetAttributeInfo("bool3x1");
            cg_setarray_type.bool3x2Attribute = cg_setarray_type.Type.GetAttributeInfo("bool3x2");
            cg_setarray_type.bool3x3Attribute = cg_setarray_type.Type.GetAttributeInfo("bool3x3");
            cg_setarray_type.bool3x4Attribute = cg_setarray_type.Type.GetAttributeInfo("bool3x4");
            cg_setarray_type.bool4x1Attribute = cg_setarray_type.Type.GetAttributeInfo("bool4x1");
            cg_setarray_type.bool4x2Attribute = cg_setarray_type.Type.GetAttributeInfo("bool4x2");
            cg_setarray_type.bool4x3Attribute = cg_setarray_type.Type.GetAttributeInfo("bool4x3");
            cg_setarray_type.bool4x4Attribute = cg_setarray_type.Type.GetAttributeInfo("bool4x4");
            cg_setarray_type.floatAttribute = cg_setarray_type.Type.GetAttributeInfo("float");
            cg_setarray_type.float1Attribute = cg_setarray_type.Type.GetAttributeInfo("float1");
            cg_setarray_type.float2Attribute = cg_setarray_type.Type.GetAttributeInfo("float2");
            cg_setarray_type.float3Attribute = cg_setarray_type.Type.GetAttributeInfo("float3");
            cg_setarray_type.float4Attribute = cg_setarray_type.Type.GetAttributeInfo("float4");
            cg_setarray_type.float1x1Attribute = cg_setarray_type.Type.GetAttributeInfo("float1x1");
            cg_setarray_type.float1x2Attribute = cg_setarray_type.Type.GetAttributeInfo("float1x2");
            cg_setarray_type.float1x3Attribute = cg_setarray_type.Type.GetAttributeInfo("float1x3");
            cg_setarray_type.float1x4Attribute = cg_setarray_type.Type.GetAttributeInfo("float1x4");
            cg_setarray_type.float2x1Attribute = cg_setarray_type.Type.GetAttributeInfo("float2x1");
            cg_setarray_type.float2x2Attribute = cg_setarray_type.Type.GetAttributeInfo("float2x2");
            cg_setarray_type.float2x3Attribute = cg_setarray_type.Type.GetAttributeInfo("float2x3");
            cg_setarray_type.float2x4Attribute = cg_setarray_type.Type.GetAttributeInfo("float2x4");
            cg_setarray_type.float3x1Attribute = cg_setarray_type.Type.GetAttributeInfo("float3x1");
            cg_setarray_type.float3x2Attribute = cg_setarray_type.Type.GetAttributeInfo("float3x2");
            cg_setarray_type.float3x3Attribute = cg_setarray_type.Type.GetAttributeInfo("float3x3");
            cg_setarray_type.float3x4Attribute = cg_setarray_type.Type.GetAttributeInfo("float3x4");
            cg_setarray_type.float4x1Attribute = cg_setarray_type.Type.GetAttributeInfo("float4x1");
            cg_setarray_type.float4x2Attribute = cg_setarray_type.Type.GetAttributeInfo("float4x2");
            cg_setarray_type.float4x3Attribute = cg_setarray_type.Type.GetAttributeInfo("float4x3");
            cg_setarray_type.float4x4Attribute = cg_setarray_type.Type.GetAttributeInfo("float4x4");
            cg_setarray_type.intAttribute = cg_setarray_type.Type.GetAttributeInfo("int");
            cg_setarray_type.int1Attribute = cg_setarray_type.Type.GetAttributeInfo("int1");
            cg_setarray_type.int2Attribute = cg_setarray_type.Type.GetAttributeInfo("int2");
            cg_setarray_type.int3Attribute = cg_setarray_type.Type.GetAttributeInfo("int3");
            cg_setarray_type.int4Attribute = cg_setarray_type.Type.GetAttributeInfo("int4");
            cg_setarray_type.int1x1Attribute = cg_setarray_type.Type.GetAttributeInfo("int1x1");
            cg_setarray_type.int1x2Attribute = cg_setarray_type.Type.GetAttributeInfo("int1x2");
            cg_setarray_type.int1x3Attribute = cg_setarray_type.Type.GetAttributeInfo("int1x3");
            cg_setarray_type.int1x4Attribute = cg_setarray_type.Type.GetAttributeInfo("int1x4");
            cg_setarray_type.int2x1Attribute = cg_setarray_type.Type.GetAttributeInfo("int2x1");
            cg_setarray_type.int2x2Attribute = cg_setarray_type.Type.GetAttributeInfo("int2x2");
            cg_setarray_type.int2x3Attribute = cg_setarray_type.Type.GetAttributeInfo("int2x3");
            cg_setarray_type.int2x4Attribute = cg_setarray_type.Type.GetAttributeInfo("int2x4");
            cg_setarray_type.int3x1Attribute = cg_setarray_type.Type.GetAttributeInfo("int3x1");
            cg_setarray_type.int3x2Attribute = cg_setarray_type.Type.GetAttributeInfo("int3x2");
            cg_setarray_type.int3x3Attribute = cg_setarray_type.Type.GetAttributeInfo("int3x3");
            cg_setarray_type.int3x4Attribute = cg_setarray_type.Type.GetAttributeInfo("int3x4");
            cg_setarray_type.int4x1Attribute = cg_setarray_type.Type.GetAttributeInfo("int4x1");
            cg_setarray_type.int4x2Attribute = cg_setarray_type.Type.GetAttributeInfo("int4x2");
            cg_setarray_type.int4x3Attribute = cg_setarray_type.Type.GetAttributeInfo("int4x3");
            cg_setarray_type.int4x4Attribute = cg_setarray_type.Type.GetAttributeInfo("int4x4");
            cg_setarray_type.halfAttribute = cg_setarray_type.Type.GetAttributeInfo("half");
            cg_setarray_type.half1Attribute = cg_setarray_type.Type.GetAttributeInfo("half1");
            cg_setarray_type.half2Attribute = cg_setarray_type.Type.GetAttributeInfo("half2");
            cg_setarray_type.half3Attribute = cg_setarray_type.Type.GetAttributeInfo("half3");
            cg_setarray_type.half4Attribute = cg_setarray_type.Type.GetAttributeInfo("half4");
            cg_setarray_type.half1x1Attribute = cg_setarray_type.Type.GetAttributeInfo("half1x1");
            cg_setarray_type.half1x2Attribute = cg_setarray_type.Type.GetAttributeInfo("half1x2");
            cg_setarray_type.half1x3Attribute = cg_setarray_type.Type.GetAttributeInfo("half1x3");
            cg_setarray_type.half1x4Attribute = cg_setarray_type.Type.GetAttributeInfo("half1x4");
            cg_setarray_type.half2x1Attribute = cg_setarray_type.Type.GetAttributeInfo("half2x1");
            cg_setarray_type.half2x2Attribute = cg_setarray_type.Type.GetAttributeInfo("half2x2");
            cg_setarray_type.half2x3Attribute = cg_setarray_type.Type.GetAttributeInfo("half2x3");
            cg_setarray_type.half2x4Attribute = cg_setarray_type.Type.GetAttributeInfo("half2x4");
            cg_setarray_type.half3x1Attribute = cg_setarray_type.Type.GetAttributeInfo("half3x1");
            cg_setarray_type.half3x2Attribute = cg_setarray_type.Type.GetAttributeInfo("half3x2");
            cg_setarray_type.half3x3Attribute = cg_setarray_type.Type.GetAttributeInfo("half3x3");
            cg_setarray_type.half3x4Attribute = cg_setarray_type.Type.GetAttributeInfo("half3x4");
            cg_setarray_type.half4x1Attribute = cg_setarray_type.Type.GetAttributeInfo("half4x1");
            cg_setarray_type.half4x2Attribute = cg_setarray_type.Type.GetAttributeInfo("half4x2");
            cg_setarray_type.half4x3Attribute = cg_setarray_type.Type.GetAttributeInfo("half4x3");
            cg_setarray_type.half4x4Attribute = cg_setarray_type.Type.GetAttributeInfo("half4x4");
            cg_setarray_type.fixedAttribute = cg_setarray_type.Type.GetAttributeInfo("fixed");
            cg_setarray_type.fixed1Attribute = cg_setarray_type.Type.GetAttributeInfo("fixed1");
            cg_setarray_type.fixed2Attribute = cg_setarray_type.Type.GetAttributeInfo("fixed2");
            cg_setarray_type.fixed3Attribute = cg_setarray_type.Type.GetAttributeInfo("fixed3");
            cg_setarray_type.fixed4Attribute = cg_setarray_type.Type.GetAttributeInfo("fixed4");
            cg_setarray_type.fixed1x1Attribute = cg_setarray_type.Type.GetAttributeInfo("fixed1x1");
            cg_setarray_type.fixed1x2Attribute = cg_setarray_type.Type.GetAttributeInfo("fixed1x2");
            cg_setarray_type.fixed1x3Attribute = cg_setarray_type.Type.GetAttributeInfo("fixed1x3");
            cg_setarray_type.fixed1x4Attribute = cg_setarray_type.Type.GetAttributeInfo("fixed1x4");
            cg_setarray_type.fixed2x1Attribute = cg_setarray_type.Type.GetAttributeInfo("fixed2x1");
            cg_setarray_type.fixed2x2Attribute = cg_setarray_type.Type.GetAttributeInfo("fixed2x2");
            cg_setarray_type.fixed2x3Attribute = cg_setarray_type.Type.GetAttributeInfo("fixed2x3");
            cg_setarray_type.fixed2x4Attribute = cg_setarray_type.Type.GetAttributeInfo("fixed2x4");
            cg_setarray_type.fixed3x1Attribute = cg_setarray_type.Type.GetAttributeInfo("fixed3x1");
            cg_setarray_type.fixed3x2Attribute = cg_setarray_type.Type.GetAttributeInfo("fixed3x2");
            cg_setarray_type.fixed3x3Attribute = cg_setarray_type.Type.GetAttributeInfo("fixed3x3");
            cg_setarray_type.fixed3x4Attribute = cg_setarray_type.Type.GetAttributeInfo("fixed3x4");
            cg_setarray_type.fixed4x1Attribute = cg_setarray_type.Type.GetAttributeInfo("fixed4x1");
            cg_setarray_type.fixed4x2Attribute = cg_setarray_type.Type.GetAttributeInfo("fixed4x2");
            cg_setarray_type.fixed4x3Attribute = cg_setarray_type.Type.GetAttributeInfo("fixed4x3");
            cg_setarray_type.fixed4x4Attribute = cg_setarray_type.Type.GetAttributeInfo("fixed4x4");
            cg_setarray_type.stringAttribute = cg_setarray_type.Type.GetAttributeInfo("string");
            cg_setarray_type.enumAttribute = cg_setarray_type.Type.GetAttributeInfo("enum");
            cg_setarray_type.lengthAttribute = cg_setarray_type.Type.GetAttributeInfo("length");
            cg_setarray_type.surfaceChild = cg_setarray_type.Type.GetChildInfo("surface");
            cg_setarray_type.sampler1DChild = cg_setarray_type.Type.GetChildInfo("sampler1D");
            cg_setarray_type.sampler2DChild = cg_setarray_type.Type.GetChildInfo("sampler2D");
            cg_setarray_type.sampler3DChild = cg_setarray_type.Type.GetChildInfo("sampler3D");
            cg_setarray_type.samplerRECTChild = cg_setarray_type.Type.GetChildInfo("samplerRECT");
            cg_setarray_type.samplerCUBEChild = cg_setarray_type.Type.GetChildInfo("samplerCUBE");
            cg_setarray_type.samplerDEPTHChild = cg_setarray_type.Type.GetChildInfo("samplerDEPTH");
            cg_setarray_type.arrayChild = cg_setarray_type.Type.GetChildInfo("array");
            cg_setarray_type.usertypeChild = cg_setarray_type.Type.GetChildInfo("usertype");

            cg_connect_param.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "cg_connect_param");
            cg_connect_param.refAttribute = cg_connect_param.Type.GetAttributeInfo("ref");

            cg_setparam.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "cg_setparam");
            cg_setparam.boolAttribute = cg_setparam.Type.GetAttributeInfo("bool");
            cg_setparam.bool1Attribute = cg_setparam.Type.GetAttributeInfo("bool1");
            cg_setparam.bool2Attribute = cg_setparam.Type.GetAttributeInfo("bool2");
            cg_setparam.bool3Attribute = cg_setparam.Type.GetAttributeInfo("bool3");
            cg_setparam.bool4Attribute = cg_setparam.Type.GetAttributeInfo("bool4");
            cg_setparam.bool1x1Attribute = cg_setparam.Type.GetAttributeInfo("bool1x1");
            cg_setparam.bool1x2Attribute = cg_setparam.Type.GetAttributeInfo("bool1x2");
            cg_setparam.bool1x3Attribute = cg_setparam.Type.GetAttributeInfo("bool1x3");
            cg_setparam.bool1x4Attribute = cg_setparam.Type.GetAttributeInfo("bool1x4");
            cg_setparam.bool2x1Attribute = cg_setparam.Type.GetAttributeInfo("bool2x1");
            cg_setparam.bool2x2Attribute = cg_setparam.Type.GetAttributeInfo("bool2x2");
            cg_setparam.bool2x3Attribute = cg_setparam.Type.GetAttributeInfo("bool2x3");
            cg_setparam.bool2x4Attribute = cg_setparam.Type.GetAttributeInfo("bool2x4");
            cg_setparam.bool3x1Attribute = cg_setparam.Type.GetAttributeInfo("bool3x1");
            cg_setparam.bool3x2Attribute = cg_setparam.Type.GetAttributeInfo("bool3x2");
            cg_setparam.bool3x3Attribute = cg_setparam.Type.GetAttributeInfo("bool3x3");
            cg_setparam.bool3x4Attribute = cg_setparam.Type.GetAttributeInfo("bool3x4");
            cg_setparam.bool4x1Attribute = cg_setparam.Type.GetAttributeInfo("bool4x1");
            cg_setparam.bool4x2Attribute = cg_setparam.Type.GetAttributeInfo("bool4x2");
            cg_setparam.bool4x3Attribute = cg_setparam.Type.GetAttributeInfo("bool4x3");
            cg_setparam.bool4x4Attribute = cg_setparam.Type.GetAttributeInfo("bool4x4");
            cg_setparam.floatAttribute = cg_setparam.Type.GetAttributeInfo("float");
            cg_setparam.float1Attribute = cg_setparam.Type.GetAttributeInfo("float1");
            cg_setparam.float2Attribute = cg_setparam.Type.GetAttributeInfo("float2");
            cg_setparam.float3Attribute = cg_setparam.Type.GetAttributeInfo("float3");
            cg_setparam.float4Attribute = cg_setparam.Type.GetAttributeInfo("float4");
            cg_setparam.float1x1Attribute = cg_setparam.Type.GetAttributeInfo("float1x1");
            cg_setparam.float1x2Attribute = cg_setparam.Type.GetAttributeInfo("float1x2");
            cg_setparam.float1x3Attribute = cg_setparam.Type.GetAttributeInfo("float1x3");
            cg_setparam.float1x4Attribute = cg_setparam.Type.GetAttributeInfo("float1x4");
            cg_setparam.float2x1Attribute = cg_setparam.Type.GetAttributeInfo("float2x1");
            cg_setparam.float2x2Attribute = cg_setparam.Type.GetAttributeInfo("float2x2");
            cg_setparam.float2x3Attribute = cg_setparam.Type.GetAttributeInfo("float2x3");
            cg_setparam.float2x4Attribute = cg_setparam.Type.GetAttributeInfo("float2x4");
            cg_setparam.float3x1Attribute = cg_setparam.Type.GetAttributeInfo("float3x1");
            cg_setparam.float3x2Attribute = cg_setparam.Type.GetAttributeInfo("float3x2");
            cg_setparam.float3x3Attribute = cg_setparam.Type.GetAttributeInfo("float3x3");
            cg_setparam.float3x4Attribute = cg_setparam.Type.GetAttributeInfo("float3x4");
            cg_setparam.float4x1Attribute = cg_setparam.Type.GetAttributeInfo("float4x1");
            cg_setparam.float4x2Attribute = cg_setparam.Type.GetAttributeInfo("float4x2");
            cg_setparam.float4x3Attribute = cg_setparam.Type.GetAttributeInfo("float4x3");
            cg_setparam.float4x4Attribute = cg_setparam.Type.GetAttributeInfo("float4x4");
            cg_setparam.intAttribute = cg_setparam.Type.GetAttributeInfo("int");
            cg_setparam.int1Attribute = cg_setparam.Type.GetAttributeInfo("int1");
            cg_setparam.int2Attribute = cg_setparam.Type.GetAttributeInfo("int2");
            cg_setparam.int3Attribute = cg_setparam.Type.GetAttributeInfo("int3");
            cg_setparam.int4Attribute = cg_setparam.Type.GetAttributeInfo("int4");
            cg_setparam.int1x1Attribute = cg_setparam.Type.GetAttributeInfo("int1x1");
            cg_setparam.int1x2Attribute = cg_setparam.Type.GetAttributeInfo("int1x2");
            cg_setparam.int1x3Attribute = cg_setparam.Type.GetAttributeInfo("int1x3");
            cg_setparam.int1x4Attribute = cg_setparam.Type.GetAttributeInfo("int1x4");
            cg_setparam.int2x1Attribute = cg_setparam.Type.GetAttributeInfo("int2x1");
            cg_setparam.int2x2Attribute = cg_setparam.Type.GetAttributeInfo("int2x2");
            cg_setparam.int2x3Attribute = cg_setparam.Type.GetAttributeInfo("int2x3");
            cg_setparam.int2x4Attribute = cg_setparam.Type.GetAttributeInfo("int2x4");
            cg_setparam.int3x1Attribute = cg_setparam.Type.GetAttributeInfo("int3x1");
            cg_setparam.int3x2Attribute = cg_setparam.Type.GetAttributeInfo("int3x2");
            cg_setparam.int3x3Attribute = cg_setparam.Type.GetAttributeInfo("int3x3");
            cg_setparam.int3x4Attribute = cg_setparam.Type.GetAttributeInfo("int3x4");
            cg_setparam.int4x1Attribute = cg_setparam.Type.GetAttributeInfo("int4x1");
            cg_setparam.int4x2Attribute = cg_setparam.Type.GetAttributeInfo("int4x2");
            cg_setparam.int4x3Attribute = cg_setparam.Type.GetAttributeInfo("int4x3");
            cg_setparam.int4x4Attribute = cg_setparam.Type.GetAttributeInfo("int4x4");
            cg_setparam.halfAttribute = cg_setparam.Type.GetAttributeInfo("half");
            cg_setparam.half1Attribute = cg_setparam.Type.GetAttributeInfo("half1");
            cg_setparam.half2Attribute = cg_setparam.Type.GetAttributeInfo("half2");
            cg_setparam.half3Attribute = cg_setparam.Type.GetAttributeInfo("half3");
            cg_setparam.half4Attribute = cg_setparam.Type.GetAttributeInfo("half4");
            cg_setparam.half1x1Attribute = cg_setparam.Type.GetAttributeInfo("half1x1");
            cg_setparam.half1x2Attribute = cg_setparam.Type.GetAttributeInfo("half1x2");
            cg_setparam.half1x3Attribute = cg_setparam.Type.GetAttributeInfo("half1x3");
            cg_setparam.half1x4Attribute = cg_setparam.Type.GetAttributeInfo("half1x4");
            cg_setparam.half2x1Attribute = cg_setparam.Type.GetAttributeInfo("half2x1");
            cg_setparam.half2x2Attribute = cg_setparam.Type.GetAttributeInfo("half2x2");
            cg_setparam.half2x3Attribute = cg_setparam.Type.GetAttributeInfo("half2x3");
            cg_setparam.half2x4Attribute = cg_setparam.Type.GetAttributeInfo("half2x4");
            cg_setparam.half3x1Attribute = cg_setparam.Type.GetAttributeInfo("half3x1");
            cg_setparam.half3x2Attribute = cg_setparam.Type.GetAttributeInfo("half3x2");
            cg_setparam.half3x3Attribute = cg_setparam.Type.GetAttributeInfo("half3x3");
            cg_setparam.half3x4Attribute = cg_setparam.Type.GetAttributeInfo("half3x4");
            cg_setparam.half4x1Attribute = cg_setparam.Type.GetAttributeInfo("half4x1");
            cg_setparam.half4x2Attribute = cg_setparam.Type.GetAttributeInfo("half4x2");
            cg_setparam.half4x3Attribute = cg_setparam.Type.GetAttributeInfo("half4x3");
            cg_setparam.half4x4Attribute = cg_setparam.Type.GetAttributeInfo("half4x4");
            cg_setparam.fixedAttribute = cg_setparam.Type.GetAttributeInfo("fixed");
            cg_setparam.fixed1Attribute = cg_setparam.Type.GetAttributeInfo("fixed1");
            cg_setparam.fixed2Attribute = cg_setparam.Type.GetAttributeInfo("fixed2");
            cg_setparam.fixed3Attribute = cg_setparam.Type.GetAttributeInfo("fixed3");
            cg_setparam.fixed4Attribute = cg_setparam.Type.GetAttributeInfo("fixed4");
            cg_setparam.fixed1x1Attribute = cg_setparam.Type.GetAttributeInfo("fixed1x1");
            cg_setparam.fixed1x2Attribute = cg_setparam.Type.GetAttributeInfo("fixed1x2");
            cg_setparam.fixed1x3Attribute = cg_setparam.Type.GetAttributeInfo("fixed1x3");
            cg_setparam.fixed1x4Attribute = cg_setparam.Type.GetAttributeInfo("fixed1x4");
            cg_setparam.fixed2x1Attribute = cg_setparam.Type.GetAttributeInfo("fixed2x1");
            cg_setparam.fixed2x2Attribute = cg_setparam.Type.GetAttributeInfo("fixed2x2");
            cg_setparam.fixed2x3Attribute = cg_setparam.Type.GetAttributeInfo("fixed2x3");
            cg_setparam.fixed2x4Attribute = cg_setparam.Type.GetAttributeInfo("fixed2x4");
            cg_setparam.fixed3x1Attribute = cg_setparam.Type.GetAttributeInfo("fixed3x1");
            cg_setparam.fixed3x2Attribute = cg_setparam.Type.GetAttributeInfo("fixed3x2");
            cg_setparam.fixed3x3Attribute = cg_setparam.Type.GetAttributeInfo("fixed3x3");
            cg_setparam.fixed3x4Attribute = cg_setparam.Type.GetAttributeInfo("fixed3x4");
            cg_setparam.fixed4x1Attribute = cg_setparam.Type.GetAttributeInfo("fixed4x1");
            cg_setparam.fixed4x2Attribute = cg_setparam.Type.GetAttributeInfo("fixed4x2");
            cg_setparam.fixed4x3Attribute = cg_setparam.Type.GetAttributeInfo("fixed4x3");
            cg_setparam.fixed4x4Attribute = cg_setparam.Type.GetAttributeInfo("fixed4x4");
            cg_setparam.stringAttribute = cg_setparam.Type.GetAttributeInfo("string");
            cg_setparam.enumAttribute = cg_setparam.Type.GetAttributeInfo("enum");
            cg_setparam.refAttribute = cg_setparam.Type.GetAttributeInfo("ref");
            cg_setparam.programAttribute = cg_setparam.Type.GetAttributeInfo("program");
            cg_setparam.surfaceChild = cg_setparam.Type.GetChildInfo("surface");
            cg_setparam.sampler1DChild = cg_setparam.Type.GetChildInfo("sampler1D");
            cg_setparam.sampler2DChild = cg_setparam.Type.GetChildInfo("sampler2D");
            cg_setparam.sampler3DChild = cg_setparam.Type.GetChildInfo("sampler3D");
            cg_setparam.samplerRECTChild = cg_setparam.Type.GetChildInfo("samplerRECT");
            cg_setparam.samplerCUBEChild = cg_setparam.Type.GetChildInfo("samplerCUBE");
            cg_setparam.samplerDEPTHChild = cg_setparam.Type.GetChildInfo("samplerDEPTH");
            cg_setparam.usertypeChild = cg_setparam.Type.GetChildInfo("usertype");
            cg_setparam.arrayChild = cg_setparam.Type.GetChildInfo("array");
            cg_setparam.connect_paramChild = cg_setparam.Type.GetChildInfo("connect_param");

            cg_newarray_type.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "cg_newarray_type");
            cg_newarray_type.boolAttribute = cg_newarray_type.Type.GetAttributeInfo("bool");
            cg_newarray_type.bool1Attribute = cg_newarray_type.Type.GetAttributeInfo("bool1");
            cg_newarray_type.bool2Attribute = cg_newarray_type.Type.GetAttributeInfo("bool2");
            cg_newarray_type.bool3Attribute = cg_newarray_type.Type.GetAttributeInfo("bool3");
            cg_newarray_type.bool4Attribute = cg_newarray_type.Type.GetAttributeInfo("bool4");
            cg_newarray_type.bool1x1Attribute = cg_newarray_type.Type.GetAttributeInfo("bool1x1");
            cg_newarray_type.bool1x2Attribute = cg_newarray_type.Type.GetAttributeInfo("bool1x2");
            cg_newarray_type.bool1x3Attribute = cg_newarray_type.Type.GetAttributeInfo("bool1x3");
            cg_newarray_type.bool1x4Attribute = cg_newarray_type.Type.GetAttributeInfo("bool1x4");
            cg_newarray_type.bool2x1Attribute = cg_newarray_type.Type.GetAttributeInfo("bool2x1");
            cg_newarray_type.bool2x2Attribute = cg_newarray_type.Type.GetAttributeInfo("bool2x2");
            cg_newarray_type.bool2x3Attribute = cg_newarray_type.Type.GetAttributeInfo("bool2x3");
            cg_newarray_type.bool2x4Attribute = cg_newarray_type.Type.GetAttributeInfo("bool2x4");
            cg_newarray_type.bool3x1Attribute = cg_newarray_type.Type.GetAttributeInfo("bool3x1");
            cg_newarray_type.bool3x2Attribute = cg_newarray_type.Type.GetAttributeInfo("bool3x2");
            cg_newarray_type.bool3x3Attribute = cg_newarray_type.Type.GetAttributeInfo("bool3x3");
            cg_newarray_type.bool3x4Attribute = cg_newarray_type.Type.GetAttributeInfo("bool3x4");
            cg_newarray_type.bool4x1Attribute = cg_newarray_type.Type.GetAttributeInfo("bool4x1");
            cg_newarray_type.bool4x2Attribute = cg_newarray_type.Type.GetAttributeInfo("bool4x2");
            cg_newarray_type.bool4x3Attribute = cg_newarray_type.Type.GetAttributeInfo("bool4x3");
            cg_newarray_type.bool4x4Attribute = cg_newarray_type.Type.GetAttributeInfo("bool4x4");
            cg_newarray_type.floatAttribute = cg_newarray_type.Type.GetAttributeInfo("float");
            cg_newarray_type.float1Attribute = cg_newarray_type.Type.GetAttributeInfo("float1");
            cg_newarray_type.float2Attribute = cg_newarray_type.Type.GetAttributeInfo("float2");
            cg_newarray_type.float3Attribute = cg_newarray_type.Type.GetAttributeInfo("float3");
            cg_newarray_type.float4Attribute = cg_newarray_type.Type.GetAttributeInfo("float4");
            cg_newarray_type.float1x1Attribute = cg_newarray_type.Type.GetAttributeInfo("float1x1");
            cg_newarray_type.float1x2Attribute = cg_newarray_type.Type.GetAttributeInfo("float1x2");
            cg_newarray_type.float1x3Attribute = cg_newarray_type.Type.GetAttributeInfo("float1x3");
            cg_newarray_type.float1x4Attribute = cg_newarray_type.Type.GetAttributeInfo("float1x4");
            cg_newarray_type.float2x1Attribute = cg_newarray_type.Type.GetAttributeInfo("float2x1");
            cg_newarray_type.float2x2Attribute = cg_newarray_type.Type.GetAttributeInfo("float2x2");
            cg_newarray_type.float2x3Attribute = cg_newarray_type.Type.GetAttributeInfo("float2x3");
            cg_newarray_type.float2x4Attribute = cg_newarray_type.Type.GetAttributeInfo("float2x4");
            cg_newarray_type.float3x1Attribute = cg_newarray_type.Type.GetAttributeInfo("float3x1");
            cg_newarray_type.float3x2Attribute = cg_newarray_type.Type.GetAttributeInfo("float3x2");
            cg_newarray_type.float3x3Attribute = cg_newarray_type.Type.GetAttributeInfo("float3x3");
            cg_newarray_type.float3x4Attribute = cg_newarray_type.Type.GetAttributeInfo("float3x4");
            cg_newarray_type.float4x1Attribute = cg_newarray_type.Type.GetAttributeInfo("float4x1");
            cg_newarray_type.float4x2Attribute = cg_newarray_type.Type.GetAttributeInfo("float4x2");
            cg_newarray_type.float4x3Attribute = cg_newarray_type.Type.GetAttributeInfo("float4x3");
            cg_newarray_type.float4x4Attribute = cg_newarray_type.Type.GetAttributeInfo("float4x4");
            cg_newarray_type.intAttribute = cg_newarray_type.Type.GetAttributeInfo("int");
            cg_newarray_type.int1Attribute = cg_newarray_type.Type.GetAttributeInfo("int1");
            cg_newarray_type.int2Attribute = cg_newarray_type.Type.GetAttributeInfo("int2");
            cg_newarray_type.int3Attribute = cg_newarray_type.Type.GetAttributeInfo("int3");
            cg_newarray_type.int4Attribute = cg_newarray_type.Type.GetAttributeInfo("int4");
            cg_newarray_type.int1x1Attribute = cg_newarray_type.Type.GetAttributeInfo("int1x1");
            cg_newarray_type.int1x2Attribute = cg_newarray_type.Type.GetAttributeInfo("int1x2");
            cg_newarray_type.int1x3Attribute = cg_newarray_type.Type.GetAttributeInfo("int1x3");
            cg_newarray_type.int1x4Attribute = cg_newarray_type.Type.GetAttributeInfo("int1x4");
            cg_newarray_type.int2x1Attribute = cg_newarray_type.Type.GetAttributeInfo("int2x1");
            cg_newarray_type.int2x2Attribute = cg_newarray_type.Type.GetAttributeInfo("int2x2");
            cg_newarray_type.int2x3Attribute = cg_newarray_type.Type.GetAttributeInfo("int2x3");
            cg_newarray_type.int2x4Attribute = cg_newarray_type.Type.GetAttributeInfo("int2x4");
            cg_newarray_type.int3x1Attribute = cg_newarray_type.Type.GetAttributeInfo("int3x1");
            cg_newarray_type.int3x2Attribute = cg_newarray_type.Type.GetAttributeInfo("int3x2");
            cg_newarray_type.int3x3Attribute = cg_newarray_type.Type.GetAttributeInfo("int3x3");
            cg_newarray_type.int3x4Attribute = cg_newarray_type.Type.GetAttributeInfo("int3x4");
            cg_newarray_type.int4x1Attribute = cg_newarray_type.Type.GetAttributeInfo("int4x1");
            cg_newarray_type.int4x2Attribute = cg_newarray_type.Type.GetAttributeInfo("int4x2");
            cg_newarray_type.int4x3Attribute = cg_newarray_type.Type.GetAttributeInfo("int4x3");
            cg_newarray_type.int4x4Attribute = cg_newarray_type.Type.GetAttributeInfo("int4x4");
            cg_newarray_type.halfAttribute = cg_newarray_type.Type.GetAttributeInfo("half");
            cg_newarray_type.half1Attribute = cg_newarray_type.Type.GetAttributeInfo("half1");
            cg_newarray_type.half2Attribute = cg_newarray_type.Type.GetAttributeInfo("half2");
            cg_newarray_type.half3Attribute = cg_newarray_type.Type.GetAttributeInfo("half3");
            cg_newarray_type.half4Attribute = cg_newarray_type.Type.GetAttributeInfo("half4");
            cg_newarray_type.half1x1Attribute = cg_newarray_type.Type.GetAttributeInfo("half1x1");
            cg_newarray_type.half1x2Attribute = cg_newarray_type.Type.GetAttributeInfo("half1x2");
            cg_newarray_type.half1x3Attribute = cg_newarray_type.Type.GetAttributeInfo("half1x3");
            cg_newarray_type.half1x4Attribute = cg_newarray_type.Type.GetAttributeInfo("half1x4");
            cg_newarray_type.half2x1Attribute = cg_newarray_type.Type.GetAttributeInfo("half2x1");
            cg_newarray_type.half2x2Attribute = cg_newarray_type.Type.GetAttributeInfo("half2x2");
            cg_newarray_type.half2x3Attribute = cg_newarray_type.Type.GetAttributeInfo("half2x3");
            cg_newarray_type.half2x4Attribute = cg_newarray_type.Type.GetAttributeInfo("half2x4");
            cg_newarray_type.half3x1Attribute = cg_newarray_type.Type.GetAttributeInfo("half3x1");
            cg_newarray_type.half3x2Attribute = cg_newarray_type.Type.GetAttributeInfo("half3x2");
            cg_newarray_type.half3x3Attribute = cg_newarray_type.Type.GetAttributeInfo("half3x3");
            cg_newarray_type.half3x4Attribute = cg_newarray_type.Type.GetAttributeInfo("half3x4");
            cg_newarray_type.half4x1Attribute = cg_newarray_type.Type.GetAttributeInfo("half4x1");
            cg_newarray_type.half4x2Attribute = cg_newarray_type.Type.GetAttributeInfo("half4x2");
            cg_newarray_type.half4x3Attribute = cg_newarray_type.Type.GetAttributeInfo("half4x3");
            cg_newarray_type.half4x4Attribute = cg_newarray_type.Type.GetAttributeInfo("half4x4");
            cg_newarray_type.fixedAttribute = cg_newarray_type.Type.GetAttributeInfo("fixed");
            cg_newarray_type.fixed1Attribute = cg_newarray_type.Type.GetAttributeInfo("fixed1");
            cg_newarray_type.fixed2Attribute = cg_newarray_type.Type.GetAttributeInfo("fixed2");
            cg_newarray_type.fixed3Attribute = cg_newarray_type.Type.GetAttributeInfo("fixed3");
            cg_newarray_type.fixed4Attribute = cg_newarray_type.Type.GetAttributeInfo("fixed4");
            cg_newarray_type.fixed1x1Attribute = cg_newarray_type.Type.GetAttributeInfo("fixed1x1");
            cg_newarray_type.fixed1x2Attribute = cg_newarray_type.Type.GetAttributeInfo("fixed1x2");
            cg_newarray_type.fixed1x3Attribute = cg_newarray_type.Type.GetAttributeInfo("fixed1x3");
            cg_newarray_type.fixed1x4Attribute = cg_newarray_type.Type.GetAttributeInfo("fixed1x4");
            cg_newarray_type.fixed2x1Attribute = cg_newarray_type.Type.GetAttributeInfo("fixed2x1");
            cg_newarray_type.fixed2x2Attribute = cg_newarray_type.Type.GetAttributeInfo("fixed2x2");
            cg_newarray_type.fixed2x3Attribute = cg_newarray_type.Type.GetAttributeInfo("fixed2x3");
            cg_newarray_type.fixed2x4Attribute = cg_newarray_type.Type.GetAttributeInfo("fixed2x4");
            cg_newarray_type.fixed3x1Attribute = cg_newarray_type.Type.GetAttributeInfo("fixed3x1");
            cg_newarray_type.fixed3x2Attribute = cg_newarray_type.Type.GetAttributeInfo("fixed3x2");
            cg_newarray_type.fixed3x3Attribute = cg_newarray_type.Type.GetAttributeInfo("fixed3x3");
            cg_newarray_type.fixed3x4Attribute = cg_newarray_type.Type.GetAttributeInfo("fixed3x4");
            cg_newarray_type.fixed4x1Attribute = cg_newarray_type.Type.GetAttributeInfo("fixed4x1");
            cg_newarray_type.fixed4x2Attribute = cg_newarray_type.Type.GetAttributeInfo("fixed4x2");
            cg_newarray_type.fixed4x3Attribute = cg_newarray_type.Type.GetAttributeInfo("fixed4x3");
            cg_newarray_type.fixed4x4Attribute = cg_newarray_type.Type.GetAttributeInfo("fixed4x4");
            cg_newarray_type.stringAttribute = cg_newarray_type.Type.GetAttributeInfo("string");
            cg_newarray_type.enumAttribute = cg_newarray_type.Type.GetAttributeInfo("enum");
            cg_newarray_type.lengthAttribute = cg_newarray_type.Type.GetAttributeInfo("length");
            cg_newarray_type.surfaceChild = cg_newarray_type.Type.GetChildInfo("surface");
            cg_newarray_type.sampler1DChild = cg_newarray_type.Type.GetChildInfo("sampler1D");
            cg_newarray_type.sampler2DChild = cg_newarray_type.Type.GetChildInfo("sampler2D");
            cg_newarray_type.sampler3DChild = cg_newarray_type.Type.GetChildInfo("sampler3D");
            cg_newarray_type.samplerRECTChild = cg_newarray_type.Type.GetChildInfo("samplerRECT");
            cg_newarray_type.samplerCUBEChild = cg_newarray_type.Type.GetChildInfo("samplerCUBE");
            cg_newarray_type.samplerDEPTHChild = cg_newarray_type.Type.GetChildInfo("samplerDEPTH");
            cg_newarray_type.arrayChild = cg_newarray_type.Type.GetChildInfo("array");
            cg_newarray_type.usertypeChild = cg_newarray_type.Type.GetChildInfo("usertype");
            cg_newarray_type.connect_paramChild = cg_newarray_type.Type.GetChildInfo("connect_param");

            profile_CG_technique.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "profile_CG_technique");
            profile_CG_technique.idAttribute = profile_CG_technique.Type.GetAttributeInfo("id");
            profile_CG_technique.sidAttribute = profile_CG_technique.Type.GetAttributeInfo("sid");
            profile_CG_technique.assetChild = profile_CG_technique.Type.GetChildInfo("asset");
            profile_CG_technique.annotateChild = profile_CG_technique.Type.GetChildInfo("annotate");
            profile_CG_technique.codeChild = profile_CG_technique.Type.GetChildInfo("code");
            profile_CG_technique.includeChild = profile_CG_technique.Type.GetChildInfo("include");
            profile_CG_technique.imageChild = profile_CG_technique.Type.GetChildInfo("image");
            profile_CG_technique.newparamChild = profile_CG_technique.Type.GetChildInfo("newparam");
            profile_CG_technique.setparamChild = profile_CG_technique.Type.GetChildInfo("setparam");
            profile_CG_technique.passChild = profile_CG_technique.Type.GetChildInfo("pass");
            profile_CG_technique.extraChild = profile_CG_technique.Type.GetChildInfo("extra");

            technique_pass.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "technique_pass");
            technique_pass.drawAttribute = technique_pass.Type.GetAttributeInfo("draw");
            technique_pass.sidAttribute = technique_pass.Type.GetAttributeInfo("sid");
            technique_pass.annotateChild = technique_pass.Type.GetChildInfo("annotate");
            technique_pass.color_targetChild = technique_pass.Type.GetChildInfo("color_target");
            technique_pass.depth_targetChild = technique_pass.Type.GetChildInfo("depth_target");
            technique_pass.stencil_targetChild = technique_pass.Type.GetChildInfo("stencil_target");
            technique_pass.color_clearChild = technique_pass.Type.GetChildInfo("color_clear");
            technique_pass.depth_clearChild = technique_pass.Type.GetChildInfo("depth_clear");
            technique_pass.stencil_clearChild = technique_pass.Type.GetChildInfo("stencil_clear");
            technique_pass.alpha_funcChild = technique_pass.Type.GetChildInfo("alpha_func");
            technique_pass.blend_funcChild = technique_pass.Type.GetChildInfo("blend_func");
            technique_pass.blend_func_separateChild = technique_pass.Type.GetChildInfo("blend_func_separate");
            technique_pass.blend_equationChild = technique_pass.Type.GetChildInfo("blend_equation");
            technique_pass.blend_equation_separateChild = technique_pass.Type.GetChildInfo("blend_equation_separate");
            technique_pass.color_materialChild = technique_pass.Type.GetChildInfo("color_material");
            technique_pass.cull_faceChild = technique_pass.Type.GetChildInfo("cull_face");
            technique_pass.depth_funcChild = technique_pass.Type.GetChildInfo("depth_func");
            technique_pass.fog_modeChild = technique_pass.Type.GetChildInfo("fog_mode");
            technique_pass.fog_coord_srcChild = technique_pass.Type.GetChildInfo("fog_coord_src");
            technique_pass.front_faceChild = technique_pass.Type.GetChildInfo("front_face");
            technique_pass.light_model_color_controlChild = technique_pass.Type.GetChildInfo("light_model_color_control");
            technique_pass.logic_opChild = technique_pass.Type.GetChildInfo("logic_op");
            technique_pass.polygon_modeChild = technique_pass.Type.GetChildInfo("polygon_mode");
            technique_pass.shade_modelChild = technique_pass.Type.GetChildInfo("shade_model");
            technique_pass.stencil_funcChild = technique_pass.Type.GetChildInfo("stencil_func");
            technique_pass.stencil_opChild = technique_pass.Type.GetChildInfo("stencil_op");
            technique_pass.stencil_func_separateChild = technique_pass.Type.GetChildInfo("stencil_func_separate");
            technique_pass.stencil_op_separateChild = technique_pass.Type.GetChildInfo("stencil_op_separate");
            technique_pass.stencil_mask_separateChild = technique_pass.Type.GetChildInfo("stencil_mask_separate");
            technique_pass.light_enableChild = technique_pass.Type.GetChildInfo("light_enable");
            technique_pass.light_ambientChild = technique_pass.Type.GetChildInfo("light_ambient");
            technique_pass.light_diffuseChild = technique_pass.Type.GetChildInfo("light_diffuse");
            technique_pass.light_specularChild = technique_pass.Type.GetChildInfo("light_specular");
            technique_pass.light_positionChild = technique_pass.Type.GetChildInfo("light_position");
            technique_pass.light_constant_attenuationChild = technique_pass.Type.GetChildInfo("light_constant_attenuation");
            technique_pass.light_linear_attenuationChild = technique_pass.Type.GetChildInfo("light_linear_attenuation");
            technique_pass.light_quadratic_attenuationChild = technique_pass.Type.GetChildInfo("light_quadratic_attenuation");
            technique_pass.light_spot_cutoffChild = technique_pass.Type.GetChildInfo("light_spot_cutoff");
            technique_pass.light_spot_directionChild = technique_pass.Type.GetChildInfo("light_spot_direction");
            technique_pass.light_spot_exponentChild = technique_pass.Type.GetChildInfo("light_spot_exponent");
            technique_pass.texture1DChild = technique_pass.Type.GetChildInfo("texture1D");
            technique_pass.texture2DChild = technique_pass.Type.GetChildInfo("texture2D");
            technique_pass.texture3DChild = technique_pass.Type.GetChildInfo("texture3D");
            technique_pass.textureCUBEChild = technique_pass.Type.GetChildInfo("textureCUBE");
            technique_pass.textureRECTChild = technique_pass.Type.GetChildInfo("textureRECT");
            technique_pass.textureDEPTHChild = technique_pass.Type.GetChildInfo("textureDEPTH");
            technique_pass.texture1D_enableChild = technique_pass.Type.GetChildInfo("texture1D_enable");
            technique_pass.texture2D_enableChild = technique_pass.Type.GetChildInfo("texture2D_enable");
            technique_pass.texture3D_enableChild = technique_pass.Type.GetChildInfo("texture3D_enable");
            technique_pass.textureCUBE_enableChild = technique_pass.Type.GetChildInfo("textureCUBE_enable");
            technique_pass.textureRECT_enableChild = technique_pass.Type.GetChildInfo("textureRECT_enable");
            technique_pass.textureDEPTH_enableChild = technique_pass.Type.GetChildInfo("textureDEPTH_enable");
            technique_pass.texture_env_colorChild = technique_pass.Type.GetChildInfo("texture_env_color");
            technique_pass.texture_env_modeChild = technique_pass.Type.GetChildInfo("texture_env_mode");
            technique_pass.clip_planeChild = technique_pass.Type.GetChildInfo("clip_plane");
            technique_pass.clip_plane_enableChild = technique_pass.Type.GetChildInfo("clip_plane_enable");
            technique_pass.blend_colorChild = technique_pass.Type.GetChildInfo("blend_color");
            technique_pass.clear_colorChild = technique_pass.Type.GetChildInfo("clear_color");
            technique_pass.clear_stencilChild = technique_pass.Type.GetChildInfo("clear_stencil");
            technique_pass.clear_depthChild = technique_pass.Type.GetChildInfo("clear_depth");
            technique_pass.color_maskChild = technique_pass.Type.GetChildInfo("color_mask");
            technique_pass.depth_boundsChild = technique_pass.Type.GetChildInfo("depth_bounds");
            technique_pass.depth_maskChild = technique_pass.Type.GetChildInfo("depth_mask");
            technique_pass.depth_rangeChild = technique_pass.Type.GetChildInfo("depth_range");
            technique_pass.fog_densityChild = technique_pass.Type.GetChildInfo("fog_density");
            technique_pass.fog_startChild = technique_pass.Type.GetChildInfo("fog_start");
            technique_pass.fog_endChild = technique_pass.Type.GetChildInfo("fog_end");
            technique_pass.fog_colorChild = technique_pass.Type.GetChildInfo("fog_color");
            technique_pass.light_model_ambientChild = technique_pass.Type.GetChildInfo("light_model_ambient");
            technique_pass.lighting_enableChild = technique_pass.Type.GetChildInfo("lighting_enable");
            technique_pass.line_stippleChild = technique_pass.Type.GetChildInfo("line_stipple");
            technique_pass.line_widthChild = technique_pass.Type.GetChildInfo("line_width");
            technique_pass.material_ambientChild = technique_pass.Type.GetChildInfo("material_ambient");
            technique_pass.material_diffuseChild = technique_pass.Type.GetChildInfo("material_diffuse");
            technique_pass.material_emissionChild = technique_pass.Type.GetChildInfo("material_emission");
            technique_pass.material_shininessChild = technique_pass.Type.GetChildInfo("material_shininess");
            technique_pass.material_specularChild = technique_pass.Type.GetChildInfo("material_specular");
            technique_pass.model_view_matrixChild = technique_pass.Type.GetChildInfo("model_view_matrix");
            technique_pass.point_distance_attenuationChild = technique_pass.Type.GetChildInfo("point_distance_attenuation");
            technique_pass.point_fade_threshold_sizeChild = technique_pass.Type.GetChildInfo("point_fade_threshold_size");
            technique_pass.point_sizeChild = technique_pass.Type.GetChildInfo("point_size");
            technique_pass.point_size_minChild = technique_pass.Type.GetChildInfo("point_size_min");
            technique_pass.point_size_maxChild = technique_pass.Type.GetChildInfo("point_size_max");
            technique_pass.polygon_offsetChild = technique_pass.Type.GetChildInfo("polygon_offset");
            technique_pass.projection_matrixChild = technique_pass.Type.GetChildInfo("projection_matrix");
            technique_pass.scissorChild = technique_pass.Type.GetChildInfo("scissor");
            technique_pass.stencil_maskChild = technique_pass.Type.GetChildInfo("stencil_mask");
            technique_pass.alpha_test_enableChild = technique_pass.Type.GetChildInfo("alpha_test_enable");
            technique_pass.auto_normal_enableChild = technique_pass.Type.GetChildInfo("auto_normal_enable");
            technique_pass.blend_enableChild = technique_pass.Type.GetChildInfo("blend_enable");
            technique_pass.color_logic_op_enableChild = technique_pass.Type.GetChildInfo("color_logic_op_enable");
            technique_pass.color_material_enableChild = technique_pass.Type.GetChildInfo("color_material_enable");
            technique_pass.cull_face_enableChild = technique_pass.Type.GetChildInfo("cull_face_enable");
            technique_pass.depth_bounds_enableChild = technique_pass.Type.GetChildInfo("depth_bounds_enable");
            technique_pass.depth_clamp_enableChild = technique_pass.Type.GetChildInfo("depth_clamp_enable");
            technique_pass.depth_test_enableChild = technique_pass.Type.GetChildInfo("depth_test_enable");
            technique_pass.dither_enableChild = technique_pass.Type.GetChildInfo("dither_enable");
            technique_pass.fog_enableChild = technique_pass.Type.GetChildInfo("fog_enable");
            technique_pass.light_model_local_viewer_enableChild = technique_pass.Type.GetChildInfo("light_model_local_viewer_enable");
            technique_pass.light_model_two_side_enableChild = technique_pass.Type.GetChildInfo("light_model_two_side_enable");
            technique_pass.line_smooth_enableChild = technique_pass.Type.GetChildInfo("line_smooth_enable");
            technique_pass.line_stipple_enableChild = technique_pass.Type.GetChildInfo("line_stipple_enable");
            technique_pass.logic_op_enableChild = technique_pass.Type.GetChildInfo("logic_op_enable");
            technique_pass.multisample_enableChild = technique_pass.Type.GetChildInfo("multisample_enable");
            technique_pass.normalize_enableChild = technique_pass.Type.GetChildInfo("normalize_enable");
            technique_pass.point_smooth_enableChild = technique_pass.Type.GetChildInfo("point_smooth_enable");
            technique_pass.polygon_offset_fill_enableChild = technique_pass.Type.GetChildInfo("polygon_offset_fill_enable");
            technique_pass.polygon_offset_line_enableChild = technique_pass.Type.GetChildInfo("polygon_offset_line_enable");
            technique_pass.polygon_offset_point_enableChild = technique_pass.Type.GetChildInfo("polygon_offset_point_enable");
            technique_pass.polygon_smooth_enableChild = technique_pass.Type.GetChildInfo("polygon_smooth_enable");
            technique_pass.polygon_stipple_enableChild = technique_pass.Type.GetChildInfo("polygon_stipple_enable");
            technique_pass.rescale_normal_enableChild = technique_pass.Type.GetChildInfo("rescale_normal_enable");
            technique_pass.sample_alpha_to_coverage_enableChild = technique_pass.Type.GetChildInfo("sample_alpha_to_coverage_enable");
            technique_pass.sample_alpha_to_one_enableChild = technique_pass.Type.GetChildInfo("sample_alpha_to_one_enable");
            technique_pass.sample_coverage_enableChild = technique_pass.Type.GetChildInfo("sample_coverage_enable");
            technique_pass.scissor_test_enableChild = technique_pass.Type.GetChildInfo("scissor_test_enable");
            technique_pass.stencil_test_enableChild = technique_pass.Type.GetChildInfo("stencil_test_enable");
            technique_pass.gl_hook_abstractChild = technique_pass.Type.GetChildInfo("gl_hook_abstract");
            technique_pass.shaderChild = technique_pass.Type.GetChildInfo("shader");
            technique_pass.extraChild = technique_pass.Type.GetChildInfo("extra");

            fx_colortarget_common.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "fx_colortarget_common");
            fx_colortarget_common.Attribute = fx_colortarget_common.Type.GetAttributeInfo("");
            fx_colortarget_common.indexAttribute = fx_colortarget_common.Type.GetAttributeInfo("index");
            fx_colortarget_common.faceAttribute = fx_colortarget_common.Type.GetAttributeInfo("face");
            fx_colortarget_common.mipAttribute = fx_colortarget_common.Type.GetAttributeInfo("mip");
            fx_colortarget_common.sliceAttribute = fx_colortarget_common.Type.GetAttributeInfo("slice");

            fx_depthtarget_common.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "fx_depthtarget_common");
            fx_depthtarget_common.Attribute = fx_depthtarget_common.Type.GetAttributeInfo("");
            fx_depthtarget_common.indexAttribute = fx_depthtarget_common.Type.GetAttributeInfo("index");
            fx_depthtarget_common.faceAttribute = fx_depthtarget_common.Type.GetAttributeInfo("face");
            fx_depthtarget_common.mipAttribute = fx_depthtarget_common.Type.GetAttributeInfo("mip");
            fx_depthtarget_common.sliceAttribute = fx_depthtarget_common.Type.GetAttributeInfo("slice");

            fx_stenciltarget_common.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "fx_stenciltarget_common");
            fx_stenciltarget_common.Attribute = fx_stenciltarget_common.Type.GetAttributeInfo("");
            fx_stenciltarget_common.indexAttribute = fx_stenciltarget_common.Type.GetAttributeInfo("index");
            fx_stenciltarget_common.faceAttribute = fx_stenciltarget_common.Type.GetAttributeInfo("face");
            fx_stenciltarget_common.mipAttribute = fx_stenciltarget_common.Type.GetAttributeInfo("mip");
            fx_stenciltarget_common.sliceAttribute = fx_stenciltarget_common.Type.GetAttributeInfo("slice");

            fx_clearcolor_common.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "fx_clearcolor_common");
            fx_clearcolor_common.Attribute = fx_clearcolor_common.Type.GetAttributeInfo("");
            fx_clearcolor_common.indexAttribute = fx_clearcolor_common.Type.GetAttributeInfo("index");

            fx_cleardepth_common.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "fx_cleardepth_common");
            fx_cleardepth_common.Attribute = fx_cleardepth_common.Type.GetAttributeInfo("");
            fx_cleardepth_common.indexAttribute = fx_cleardepth_common.Type.GetAttributeInfo("index");

            fx_clearstencil_common.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "fx_clearstencil_common");
            fx_clearstencil_common.Attribute = fx_clearstencil_common.Type.GetAttributeInfo("");
            fx_clearstencil_common.indexAttribute = fx_clearstencil_common.Type.GetAttributeInfo("index");

            alpha_func.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "alpha_func");
            alpha_func.funcChild = alpha_func.Type.GetChildInfo("func");
            alpha_func.valueChild = alpha_func.Type.GetChildInfo("value");

            alpha_func_func.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "alpha_func_func");
            alpha_func_func.valueAttribute = alpha_func_func.Type.GetAttributeInfo("value");
            alpha_func_func.paramAttribute = alpha_func_func.Type.GetAttributeInfo("param");

            alpha_func_value.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "alpha_func_value");
            alpha_func_value.valueAttribute = alpha_func_value.Type.GetAttributeInfo("value");
            alpha_func_value.paramAttribute = alpha_func_value.Type.GetAttributeInfo("param");

            blend_func.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "blend_func");
            blend_func.srcChild = blend_func.Type.GetChildInfo("src");
            blend_func.destChild = blend_func.Type.GetChildInfo("dest");

            blend_func_src.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "blend_func_src");
            blend_func_src.valueAttribute = blend_func_src.Type.GetAttributeInfo("value");
            blend_func_src.paramAttribute = blend_func_src.Type.GetAttributeInfo("param");

            blend_func_dest.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "blend_func_dest");
            blend_func_dest.valueAttribute = blend_func_dest.Type.GetAttributeInfo("value");
            blend_func_dest.paramAttribute = blend_func_dest.Type.GetAttributeInfo("param");

            blend_func_separate.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "blend_func_separate");
            blend_func_separate.src_rgbChild = blend_func_separate.Type.GetChildInfo("src_rgb");
            blend_func_separate.dest_rgbChild = blend_func_separate.Type.GetChildInfo("dest_rgb");
            blend_func_separate.src_alphaChild = blend_func_separate.Type.GetChildInfo("src_alpha");
            blend_func_separate.dest_alphaChild = blend_func_separate.Type.GetChildInfo("dest_alpha");

            blend_func_separate_src_rgb.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "blend_func_separate_src_rgb");
            blend_func_separate_src_rgb.valueAttribute = blend_func_separate_src_rgb.Type.GetAttributeInfo("value");
            blend_func_separate_src_rgb.paramAttribute = blend_func_separate_src_rgb.Type.GetAttributeInfo("param");

            blend_func_separate_dest_rgb.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "blend_func_separate_dest_rgb");
            blend_func_separate_dest_rgb.valueAttribute = blend_func_separate_dest_rgb.Type.GetAttributeInfo("value");
            blend_func_separate_dest_rgb.paramAttribute = blend_func_separate_dest_rgb.Type.GetAttributeInfo("param");

            blend_func_separate_src_alpha.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "blend_func_separate_src_alpha");
            blend_func_separate_src_alpha.valueAttribute = blend_func_separate_src_alpha.Type.GetAttributeInfo("value");
            blend_func_separate_src_alpha.paramAttribute = blend_func_separate_src_alpha.Type.GetAttributeInfo("param");

            blend_func_separate_dest_alpha.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "blend_func_separate_dest_alpha");
            blend_func_separate_dest_alpha.valueAttribute = blend_func_separate_dest_alpha.Type.GetAttributeInfo("value");
            blend_func_separate_dest_alpha.paramAttribute = blend_func_separate_dest_alpha.Type.GetAttributeInfo("param");

            blend_equation.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "blend_equation");
            blend_equation.valueAttribute = blend_equation.Type.GetAttributeInfo("value");
            blend_equation.paramAttribute = blend_equation.Type.GetAttributeInfo("param");

            blend_equation_separate.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "blend_equation_separate");
            blend_equation_separate.rgbChild = blend_equation_separate.Type.GetChildInfo("rgb");
            blend_equation_separate.alphaChild = blend_equation_separate.Type.GetChildInfo("alpha");

            blend_equation_separate_rgb.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "blend_equation_separate_rgb");
            blend_equation_separate_rgb.valueAttribute = blend_equation_separate_rgb.Type.GetAttributeInfo("value");
            blend_equation_separate_rgb.paramAttribute = blend_equation_separate_rgb.Type.GetAttributeInfo("param");

            blend_equation_separate_alpha.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "blend_equation_separate_alpha");
            blend_equation_separate_alpha.valueAttribute = blend_equation_separate_alpha.Type.GetAttributeInfo("value");
            blend_equation_separate_alpha.paramAttribute = blend_equation_separate_alpha.Type.GetAttributeInfo("param");

            color_material.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "color_material");
            color_material.faceChild = color_material.Type.GetChildInfo("face");
            color_material.modeChild = color_material.Type.GetChildInfo("mode");

            color_material_face.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "color_material_face");
            color_material_face.valueAttribute = color_material_face.Type.GetAttributeInfo("value");
            color_material_face.paramAttribute = color_material_face.Type.GetAttributeInfo("param");

            color_material_mode.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "color_material_mode");
            color_material_mode.valueAttribute = color_material_mode.Type.GetAttributeInfo("value");
            color_material_mode.paramAttribute = color_material_mode.Type.GetAttributeInfo("param");

            cull_face.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "cull_face");
            cull_face.valueAttribute = cull_face.Type.GetAttributeInfo("value");
            cull_face.paramAttribute = cull_face.Type.GetAttributeInfo("param");

            depth_func.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "depth_func");
            depth_func.valueAttribute = depth_func.Type.GetAttributeInfo("value");
            depth_func.paramAttribute = depth_func.Type.GetAttributeInfo("param");

            fog_mode.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "fog_mode");
            fog_mode.valueAttribute = fog_mode.Type.GetAttributeInfo("value");
            fog_mode.paramAttribute = fog_mode.Type.GetAttributeInfo("param");

            fog_coord_src.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "fog_coord_src");
            fog_coord_src.valueAttribute = fog_coord_src.Type.GetAttributeInfo("value");
            fog_coord_src.paramAttribute = fog_coord_src.Type.GetAttributeInfo("param");

            front_face.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "front_face");
            front_face.valueAttribute = front_face.Type.GetAttributeInfo("value");
            front_face.paramAttribute = front_face.Type.GetAttributeInfo("param");

            light_model_color_control.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "light_model_color_control");
            light_model_color_control.valueAttribute = light_model_color_control.Type.GetAttributeInfo("value");
            light_model_color_control.paramAttribute = light_model_color_control.Type.GetAttributeInfo("param");

            logic_op.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "logic_op");
            logic_op.valueAttribute = logic_op.Type.GetAttributeInfo("value");
            logic_op.paramAttribute = logic_op.Type.GetAttributeInfo("param");

            polygon_mode.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "polygon_mode");
            polygon_mode.faceChild = polygon_mode.Type.GetChildInfo("face");
            polygon_mode.modeChild = polygon_mode.Type.GetChildInfo("mode");

            polygon_mode_face.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "polygon_mode_face");
            polygon_mode_face.valueAttribute = polygon_mode_face.Type.GetAttributeInfo("value");
            polygon_mode_face.paramAttribute = polygon_mode_face.Type.GetAttributeInfo("param");

            polygon_mode_mode.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "polygon_mode_mode");
            polygon_mode_mode.valueAttribute = polygon_mode_mode.Type.GetAttributeInfo("value");
            polygon_mode_mode.paramAttribute = polygon_mode_mode.Type.GetAttributeInfo("param");

            shade_model.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "shade_model");
            shade_model.valueAttribute = shade_model.Type.GetAttributeInfo("value");
            shade_model.paramAttribute = shade_model.Type.GetAttributeInfo("param");

            stencil_func.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "stencil_func");
            stencil_func.funcChild = stencil_func.Type.GetChildInfo("func");
            stencil_func.refChild = stencil_func.Type.GetChildInfo("ref");
            stencil_func.maskChild = stencil_func.Type.GetChildInfo("mask");

            stencil_func_func.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "stencil_func_func");
            stencil_func_func.valueAttribute = stencil_func_func.Type.GetAttributeInfo("value");
            stencil_func_func.paramAttribute = stencil_func_func.Type.GetAttributeInfo("param");

            stencil_func_ref.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "stencil_func_ref");
            stencil_func_ref.valueAttribute = stencil_func_ref.Type.GetAttributeInfo("value");
            stencil_func_ref.paramAttribute = stencil_func_ref.Type.GetAttributeInfo("param");

            stencil_func_mask.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "stencil_func_mask");
            stencil_func_mask.valueAttribute = stencil_func_mask.Type.GetAttributeInfo("value");
            stencil_func_mask.paramAttribute = stencil_func_mask.Type.GetAttributeInfo("param");

            stencil_op.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "stencil_op");
            stencil_op.failChild = stencil_op.Type.GetChildInfo("fail");
            stencil_op.zfailChild = stencil_op.Type.GetChildInfo("zfail");
            stencil_op.zpassChild = stencil_op.Type.GetChildInfo("zpass");

            stencil_op_fail.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "stencil_op_fail");
            stencil_op_fail.valueAttribute = stencil_op_fail.Type.GetAttributeInfo("value");
            stencil_op_fail.paramAttribute = stencil_op_fail.Type.GetAttributeInfo("param");

            stencil_op_zfail.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "stencil_op_zfail");
            stencil_op_zfail.valueAttribute = stencil_op_zfail.Type.GetAttributeInfo("value");
            stencil_op_zfail.paramAttribute = stencil_op_zfail.Type.GetAttributeInfo("param");

            stencil_op_zpass.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "stencil_op_zpass");
            stencil_op_zpass.valueAttribute = stencil_op_zpass.Type.GetAttributeInfo("value");
            stencil_op_zpass.paramAttribute = stencil_op_zpass.Type.GetAttributeInfo("param");

            stencil_func_separate.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "stencil_func_separate");
            stencil_func_separate.frontChild = stencil_func_separate.Type.GetChildInfo("front");
            stencil_func_separate.backChild = stencil_func_separate.Type.GetChildInfo("back");
            stencil_func_separate.refChild = stencil_func_separate.Type.GetChildInfo("ref");
            stencil_func_separate.maskChild = stencil_func_separate.Type.GetChildInfo("mask");

            stencil_func_separate_front.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "stencil_func_separate_front");
            stencil_func_separate_front.valueAttribute = stencil_func_separate_front.Type.GetAttributeInfo("value");
            stencil_func_separate_front.paramAttribute = stencil_func_separate_front.Type.GetAttributeInfo("param");

            stencil_func_separate_back.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "stencil_func_separate_back");
            stencil_func_separate_back.valueAttribute = stencil_func_separate_back.Type.GetAttributeInfo("value");
            stencil_func_separate_back.paramAttribute = stencil_func_separate_back.Type.GetAttributeInfo("param");

            stencil_func_separate_ref.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "stencil_func_separate_ref");
            stencil_func_separate_ref.valueAttribute = stencil_func_separate_ref.Type.GetAttributeInfo("value");
            stencil_func_separate_ref.paramAttribute = stencil_func_separate_ref.Type.GetAttributeInfo("param");

            stencil_func_separate_mask.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "stencil_func_separate_mask");
            stencil_func_separate_mask.valueAttribute = stencil_func_separate_mask.Type.GetAttributeInfo("value");
            stencil_func_separate_mask.paramAttribute = stencil_func_separate_mask.Type.GetAttributeInfo("param");

            stencil_op_separate.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "stencil_op_separate");
            stencil_op_separate.faceChild = stencil_op_separate.Type.GetChildInfo("face");
            stencil_op_separate.failChild = stencil_op_separate.Type.GetChildInfo("fail");
            stencil_op_separate.zfailChild = stencil_op_separate.Type.GetChildInfo("zfail");
            stencil_op_separate.zpassChild = stencil_op_separate.Type.GetChildInfo("zpass");

            stencil_op_separate_face.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "stencil_op_separate_face");
            stencil_op_separate_face.valueAttribute = stencil_op_separate_face.Type.GetAttributeInfo("value");
            stencil_op_separate_face.paramAttribute = stencil_op_separate_face.Type.GetAttributeInfo("param");

            stencil_op_separate_fail.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "stencil_op_separate_fail");
            stencil_op_separate_fail.valueAttribute = stencil_op_separate_fail.Type.GetAttributeInfo("value");
            stencil_op_separate_fail.paramAttribute = stencil_op_separate_fail.Type.GetAttributeInfo("param");

            stencil_op_separate_zfail.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "stencil_op_separate_zfail");
            stencil_op_separate_zfail.valueAttribute = stencil_op_separate_zfail.Type.GetAttributeInfo("value");
            stencil_op_separate_zfail.paramAttribute = stencil_op_separate_zfail.Type.GetAttributeInfo("param");

            stencil_op_separate_zpass.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "stencil_op_separate_zpass");
            stencil_op_separate_zpass.valueAttribute = stencil_op_separate_zpass.Type.GetAttributeInfo("value");
            stencil_op_separate_zpass.paramAttribute = stencil_op_separate_zpass.Type.GetAttributeInfo("param");

            stencil_mask_separate.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "stencil_mask_separate");
            stencil_mask_separate.faceChild = stencil_mask_separate.Type.GetChildInfo("face");
            stencil_mask_separate.maskChild = stencil_mask_separate.Type.GetChildInfo("mask");

            stencil_mask_separate_face.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "stencil_mask_separate_face");
            stencil_mask_separate_face.valueAttribute = stencil_mask_separate_face.Type.GetAttributeInfo("value");
            stencil_mask_separate_face.paramAttribute = stencil_mask_separate_face.Type.GetAttributeInfo("param");

            stencil_mask_separate_mask.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "stencil_mask_separate_mask");
            stencil_mask_separate_mask.valueAttribute = stencil_mask_separate_mask.Type.GetAttributeInfo("value");
            stencil_mask_separate_mask.paramAttribute = stencil_mask_separate_mask.Type.GetAttributeInfo("param");

            light_enable.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "light_enable");
            light_enable.valueAttribute = light_enable.Type.GetAttributeInfo("value");
            light_enable.paramAttribute = light_enable.Type.GetAttributeInfo("param");
            light_enable.indexAttribute = light_enable.Type.GetAttributeInfo("index");

            light_ambient.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "light_ambient");
            light_ambient.valueAttribute = light_ambient.Type.GetAttributeInfo("value");
            light_ambient.paramAttribute = light_ambient.Type.GetAttributeInfo("param");
            light_ambient.indexAttribute = light_ambient.Type.GetAttributeInfo("index");

            light_diffuse.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "light_diffuse");
            light_diffuse.valueAttribute = light_diffuse.Type.GetAttributeInfo("value");
            light_diffuse.paramAttribute = light_diffuse.Type.GetAttributeInfo("param");
            light_diffuse.indexAttribute = light_diffuse.Type.GetAttributeInfo("index");

            light_specular.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "light_specular");
            light_specular.valueAttribute = light_specular.Type.GetAttributeInfo("value");
            light_specular.paramAttribute = light_specular.Type.GetAttributeInfo("param");
            light_specular.indexAttribute = light_specular.Type.GetAttributeInfo("index");

            light_position.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "light_position");
            light_position.valueAttribute = light_position.Type.GetAttributeInfo("value");
            light_position.paramAttribute = light_position.Type.GetAttributeInfo("param");
            light_position.indexAttribute = light_position.Type.GetAttributeInfo("index");

            light_constant_attenuation.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "light_constant_attenuation");
            light_constant_attenuation.valueAttribute = light_constant_attenuation.Type.GetAttributeInfo("value");
            light_constant_attenuation.paramAttribute = light_constant_attenuation.Type.GetAttributeInfo("param");
            light_constant_attenuation.indexAttribute = light_constant_attenuation.Type.GetAttributeInfo("index");

            light_linear_attenuation.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "light_linear_attenuation");
            light_linear_attenuation.valueAttribute = light_linear_attenuation.Type.GetAttributeInfo("value");
            light_linear_attenuation.paramAttribute = light_linear_attenuation.Type.GetAttributeInfo("param");
            light_linear_attenuation.indexAttribute = light_linear_attenuation.Type.GetAttributeInfo("index");

            light_quadratic_attenuation.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "light_quadratic_attenuation");
            light_quadratic_attenuation.valueAttribute = light_quadratic_attenuation.Type.GetAttributeInfo("value");
            light_quadratic_attenuation.paramAttribute = light_quadratic_attenuation.Type.GetAttributeInfo("param");
            light_quadratic_attenuation.indexAttribute = light_quadratic_attenuation.Type.GetAttributeInfo("index");

            light_spot_cutoff.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "light_spot_cutoff");
            light_spot_cutoff.valueAttribute = light_spot_cutoff.Type.GetAttributeInfo("value");
            light_spot_cutoff.paramAttribute = light_spot_cutoff.Type.GetAttributeInfo("param");
            light_spot_cutoff.indexAttribute = light_spot_cutoff.Type.GetAttributeInfo("index");

            light_spot_direction.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "light_spot_direction");
            light_spot_direction.valueAttribute = light_spot_direction.Type.GetAttributeInfo("value");
            light_spot_direction.paramAttribute = light_spot_direction.Type.GetAttributeInfo("param");
            light_spot_direction.indexAttribute = light_spot_direction.Type.GetAttributeInfo("index");

            light_spot_exponent.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "light_spot_exponent");
            light_spot_exponent.valueAttribute = light_spot_exponent.Type.GetAttributeInfo("value");
            light_spot_exponent.paramAttribute = light_spot_exponent.Type.GetAttributeInfo("param");
            light_spot_exponent.indexAttribute = light_spot_exponent.Type.GetAttributeInfo("index");

            texture1D.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "texture1D");
            texture1D.paramAttribute = texture1D.Type.GetAttributeInfo("param");
            texture1D.indexAttribute = texture1D.Type.GetAttributeInfo("index");
            texture1D.valueChild = texture1D.Type.GetChildInfo("value");

            gl_sampler1D.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "gl_sampler1D");
            gl_sampler1D.sourceAttribute = gl_sampler1D.Type.GetAttributeInfo("source");
            gl_sampler1D.wrap_sAttribute = gl_sampler1D.Type.GetAttributeInfo("wrap_s");
            gl_sampler1D.minfilterAttribute = gl_sampler1D.Type.GetAttributeInfo("minfilter");
            gl_sampler1D.magfilterAttribute = gl_sampler1D.Type.GetAttributeInfo("magfilter");
            gl_sampler1D.mipfilterAttribute = gl_sampler1D.Type.GetAttributeInfo("mipfilter");
            gl_sampler1D.border_colorAttribute = gl_sampler1D.Type.GetAttributeInfo("border_color");
            gl_sampler1D.mipmap_maxlevelAttribute = gl_sampler1D.Type.GetAttributeInfo("mipmap_maxlevel");
            gl_sampler1D.mipmap_biasAttribute = gl_sampler1D.Type.GetAttributeInfo("mipmap_bias");
            gl_sampler1D.extraChild = gl_sampler1D.Type.GetChildInfo("extra");

            texture2D.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "texture2D");
            texture2D.paramAttribute = texture2D.Type.GetAttributeInfo("param");
            texture2D.indexAttribute = texture2D.Type.GetAttributeInfo("index");
            texture2D.valueChild = texture2D.Type.GetChildInfo("value");

            gl_sampler2D.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "gl_sampler2D");
            gl_sampler2D.sourceAttribute = gl_sampler2D.Type.GetAttributeInfo("source");
            gl_sampler2D.wrap_sAttribute = gl_sampler2D.Type.GetAttributeInfo("wrap_s");
            gl_sampler2D.wrap_tAttribute = gl_sampler2D.Type.GetAttributeInfo("wrap_t");
            gl_sampler2D.minfilterAttribute = gl_sampler2D.Type.GetAttributeInfo("minfilter");
            gl_sampler2D.magfilterAttribute = gl_sampler2D.Type.GetAttributeInfo("magfilter");
            gl_sampler2D.mipfilterAttribute = gl_sampler2D.Type.GetAttributeInfo("mipfilter");
            gl_sampler2D.border_colorAttribute = gl_sampler2D.Type.GetAttributeInfo("border_color");
            gl_sampler2D.mipmap_maxlevelAttribute = gl_sampler2D.Type.GetAttributeInfo("mipmap_maxlevel");
            gl_sampler2D.mipmap_biasAttribute = gl_sampler2D.Type.GetAttributeInfo("mipmap_bias");
            gl_sampler2D.extraChild = gl_sampler2D.Type.GetChildInfo("extra");

            texture3D.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "texture3D");
            texture3D.paramAttribute = texture3D.Type.GetAttributeInfo("param");
            texture3D.indexAttribute = texture3D.Type.GetAttributeInfo("index");
            texture3D.valueChild = texture3D.Type.GetChildInfo("value");

            gl_sampler3D.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "gl_sampler3D");
            gl_sampler3D.sourceAttribute = gl_sampler3D.Type.GetAttributeInfo("source");
            gl_sampler3D.wrap_sAttribute = gl_sampler3D.Type.GetAttributeInfo("wrap_s");
            gl_sampler3D.wrap_tAttribute = gl_sampler3D.Type.GetAttributeInfo("wrap_t");
            gl_sampler3D.wrap_pAttribute = gl_sampler3D.Type.GetAttributeInfo("wrap_p");
            gl_sampler3D.minfilterAttribute = gl_sampler3D.Type.GetAttributeInfo("minfilter");
            gl_sampler3D.magfilterAttribute = gl_sampler3D.Type.GetAttributeInfo("magfilter");
            gl_sampler3D.mipfilterAttribute = gl_sampler3D.Type.GetAttributeInfo("mipfilter");
            gl_sampler3D.border_colorAttribute = gl_sampler3D.Type.GetAttributeInfo("border_color");
            gl_sampler3D.mipmap_maxlevelAttribute = gl_sampler3D.Type.GetAttributeInfo("mipmap_maxlevel");
            gl_sampler3D.mipmap_biasAttribute = gl_sampler3D.Type.GetAttributeInfo("mipmap_bias");
            gl_sampler3D.extraChild = gl_sampler3D.Type.GetChildInfo("extra");

            textureCUBE.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "textureCUBE");
            textureCUBE.paramAttribute = textureCUBE.Type.GetAttributeInfo("param");
            textureCUBE.indexAttribute = textureCUBE.Type.GetAttributeInfo("index");
            textureCUBE.valueChild = textureCUBE.Type.GetChildInfo("value");

            gl_samplerCUBE.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "gl_samplerCUBE");
            gl_samplerCUBE.sourceAttribute = gl_samplerCUBE.Type.GetAttributeInfo("source");
            gl_samplerCUBE.wrap_sAttribute = gl_samplerCUBE.Type.GetAttributeInfo("wrap_s");
            gl_samplerCUBE.wrap_tAttribute = gl_samplerCUBE.Type.GetAttributeInfo("wrap_t");
            gl_samplerCUBE.wrap_pAttribute = gl_samplerCUBE.Type.GetAttributeInfo("wrap_p");
            gl_samplerCUBE.minfilterAttribute = gl_samplerCUBE.Type.GetAttributeInfo("minfilter");
            gl_samplerCUBE.magfilterAttribute = gl_samplerCUBE.Type.GetAttributeInfo("magfilter");
            gl_samplerCUBE.mipfilterAttribute = gl_samplerCUBE.Type.GetAttributeInfo("mipfilter");
            gl_samplerCUBE.border_colorAttribute = gl_samplerCUBE.Type.GetAttributeInfo("border_color");
            gl_samplerCUBE.mipmap_maxlevelAttribute = gl_samplerCUBE.Type.GetAttributeInfo("mipmap_maxlevel");
            gl_samplerCUBE.mipmap_biasAttribute = gl_samplerCUBE.Type.GetAttributeInfo("mipmap_bias");
            gl_samplerCUBE.extraChild = gl_samplerCUBE.Type.GetChildInfo("extra");

            textureRECT.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "textureRECT");
            textureRECT.paramAttribute = textureRECT.Type.GetAttributeInfo("param");
            textureRECT.indexAttribute = textureRECT.Type.GetAttributeInfo("index");
            textureRECT.valueChild = textureRECT.Type.GetChildInfo("value");

            gl_samplerRECT.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "gl_samplerRECT");
            gl_samplerRECT.sourceAttribute = gl_samplerRECT.Type.GetAttributeInfo("source");
            gl_samplerRECT.wrap_sAttribute = gl_samplerRECT.Type.GetAttributeInfo("wrap_s");
            gl_samplerRECT.wrap_tAttribute = gl_samplerRECT.Type.GetAttributeInfo("wrap_t");
            gl_samplerRECT.minfilterAttribute = gl_samplerRECT.Type.GetAttributeInfo("minfilter");
            gl_samplerRECT.magfilterAttribute = gl_samplerRECT.Type.GetAttributeInfo("magfilter");
            gl_samplerRECT.mipfilterAttribute = gl_samplerRECT.Type.GetAttributeInfo("mipfilter");
            gl_samplerRECT.border_colorAttribute = gl_samplerRECT.Type.GetAttributeInfo("border_color");
            gl_samplerRECT.mipmap_maxlevelAttribute = gl_samplerRECT.Type.GetAttributeInfo("mipmap_maxlevel");
            gl_samplerRECT.mipmap_biasAttribute = gl_samplerRECT.Type.GetAttributeInfo("mipmap_bias");
            gl_samplerRECT.extraChild = gl_samplerRECT.Type.GetChildInfo("extra");

            textureDEPTH.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "textureDEPTH");
            textureDEPTH.paramAttribute = textureDEPTH.Type.GetAttributeInfo("param");
            textureDEPTH.indexAttribute = textureDEPTH.Type.GetAttributeInfo("index");
            textureDEPTH.valueChild = textureDEPTH.Type.GetChildInfo("value");

            gl_samplerDEPTH.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "gl_samplerDEPTH");
            gl_samplerDEPTH.sourceAttribute = gl_samplerDEPTH.Type.GetAttributeInfo("source");
            gl_samplerDEPTH.wrap_sAttribute = gl_samplerDEPTH.Type.GetAttributeInfo("wrap_s");
            gl_samplerDEPTH.wrap_tAttribute = gl_samplerDEPTH.Type.GetAttributeInfo("wrap_t");
            gl_samplerDEPTH.minfilterAttribute = gl_samplerDEPTH.Type.GetAttributeInfo("minfilter");
            gl_samplerDEPTH.magfilterAttribute = gl_samplerDEPTH.Type.GetAttributeInfo("magfilter");
            gl_samplerDEPTH.extraChild = gl_samplerDEPTH.Type.GetChildInfo("extra");

            texture1D_enable.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "texture1D_enable");
            texture1D_enable.valueAttribute = texture1D_enable.Type.GetAttributeInfo("value");
            texture1D_enable.paramAttribute = texture1D_enable.Type.GetAttributeInfo("param");
            texture1D_enable.indexAttribute = texture1D_enable.Type.GetAttributeInfo("index");

            texture2D_enable.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "texture2D_enable");
            texture2D_enable.valueAttribute = texture2D_enable.Type.GetAttributeInfo("value");
            texture2D_enable.paramAttribute = texture2D_enable.Type.GetAttributeInfo("param");
            texture2D_enable.indexAttribute = texture2D_enable.Type.GetAttributeInfo("index");

            texture3D_enable.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "texture3D_enable");
            texture3D_enable.valueAttribute = texture3D_enable.Type.GetAttributeInfo("value");
            texture3D_enable.paramAttribute = texture3D_enable.Type.GetAttributeInfo("param");
            texture3D_enable.indexAttribute = texture3D_enable.Type.GetAttributeInfo("index");

            textureCUBE_enable.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "textureCUBE_enable");
            textureCUBE_enable.valueAttribute = textureCUBE_enable.Type.GetAttributeInfo("value");
            textureCUBE_enable.paramAttribute = textureCUBE_enable.Type.GetAttributeInfo("param");
            textureCUBE_enable.indexAttribute = textureCUBE_enable.Type.GetAttributeInfo("index");

            textureRECT_enable.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "textureRECT_enable");
            textureRECT_enable.valueAttribute = textureRECT_enable.Type.GetAttributeInfo("value");
            textureRECT_enable.paramAttribute = textureRECT_enable.Type.GetAttributeInfo("param");
            textureRECT_enable.indexAttribute = textureRECT_enable.Type.GetAttributeInfo("index");

            textureDEPTH_enable.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "textureDEPTH_enable");
            textureDEPTH_enable.valueAttribute = textureDEPTH_enable.Type.GetAttributeInfo("value");
            textureDEPTH_enable.paramAttribute = textureDEPTH_enable.Type.GetAttributeInfo("param");
            textureDEPTH_enable.indexAttribute = textureDEPTH_enable.Type.GetAttributeInfo("index");

            texture_env_color.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "texture_env_color");
            texture_env_color.valueAttribute = texture_env_color.Type.GetAttributeInfo("value");
            texture_env_color.paramAttribute = texture_env_color.Type.GetAttributeInfo("param");
            texture_env_color.indexAttribute = texture_env_color.Type.GetAttributeInfo("index");

            texture_env_mode.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "texture_env_mode");
            texture_env_mode.valueAttribute = texture_env_mode.Type.GetAttributeInfo("value");
            texture_env_mode.paramAttribute = texture_env_mode.Type.GetAttributeInfo("param");
            texture_env_mode.indexAttribute = texture_env_mode.Type.GetAttributeInfo("index");

            clip_plane.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "clip_plane");
            clip_plane.valueAttribute = clip_plane.Type.GetAttributeInfo("value");
            clip_plane.paramAttribute = clip_plane.Type.GetAttributeInfo("param");
            clip_plane.indexAttribute = clip_plane.Type.GetAttributeInfo("index");

            clip_plane_enable.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "clip_plane_enable");
            clip_plane_enable.valueAttribute = clip_plane_enable.Type.GetAttributeInfo("value");
            clip_plane_enable.paramAttribute = clip_plane_enable.Type.GetAttributeInfo("param");
            clip_plane_enable.indexAttribute = clip_plane_enable.Type.GetAttributeInfo("index");

            blend_color.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "blend_color");
            blend_color.valueAttribute = blend_color.Type.GetAttributeInfo("value");
            blend_color.paramAttribute = blend_color.Type.GetAttributeInfo("param");

            clear_color.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "clear_color");
            clear_color.valueAttribute = clear_color.Type.GetAttributeInfo("value");
            clear_color.paramAttribute = clear_color.Type.GetAttributeInfo("param");

            clear_stencil.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "clear_stencil");
            clear_stencil.valueAttribute = clear_stencil.Type.GetAttributeInfo("value");
            clear_stencil.paramAttribute = clear_stencil.Type.GetAttributeInfo("param");

            clear_depth.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "clear_depth");
            clear_depth.valueAttribute = clear_depth.Type.GetAttributeInfo("value");
            clear_depth.paramAttribute = clear_depth.Type.GetAttributeInfo("param");

            color_mask.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "color_mask");
            color_mask.valueAttribute = color_mask.Type.GetAttributeInfo("value");
            color_mask.paramAttribute = color_mask.Type.GetAttributeInfo("param");

            depth_bounds.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "depth_bounds");
            depth_bounds.valueAttribute = depth_bounds.Type.GetAttributeInfo("value");
            depth_bounds.paramAttribute = depth_bounds.Type.GetAttributeInfo("param");

            depth_mask.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "depth_mask");
            depth_mask.valueAttribute = depth_mask.Type.GetAttributeInfo("value");
            depth_mask.paramAttribute = depth_mask.Type.GetAttributeInfo("param");

            depth_range.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "depth_range");
            depth_range.valueAttribute = depth_range.Type.GetAttributeInfo("value");
            depth_range.paramAttribute = depth_range.Type.GetAttributeInfo("param");

            fog_density.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "fog_density");
            fog_density.valueAttribute = fog_density.Type.GetAttributeInfo("value");
            fog_density.paramAttribute = fog_density.Type.GetAttributeInfo("param");

            fog_start.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "fog_start");
            fog_start.valueAttribute = fog_start.Type.GetAttributeInfo("value");
            fog_start.paramAttribute = fog_start.Type.GetAttributeInfo("param");

            fog_end.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "fog_end");
            fog_end.valueAttribute = fog_end.Type.GetAttributeInfo("value");
            fog_end.paramAttribute = fog_end.Type.GetAttributeInfo("param");

            fog_color.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "fog_color");
            fog_color.valueAttribute = fog_color.Type.GetAttributeInfo("value");
            fog_color.paramAttribute = fog_color.Type.GetAttributeInfo("param");

            light_model_ambient.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "light_model_ambient");
            light_model_ambient.valueAttribute = light_model_ambient.Type.GetAttributeInfo("value");
            light_model_ambient.paramAttribute = light_model_ambient.Type.GetAttributeInfo("param");

            lighting_enable.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "lighting_enable");
            lighting_enable.valueAttribute = lighting_enable.Type.GetAttributeInfo("value");
            lighting_enable.paramAttribute = lighting_enable.Type.GetAttributeInfo("param");

            line_stipple.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "line_stipple");
            line_stipple.valueAttribute = line_stipple.Type.GetAttributeInfo("value");
            line_stipple.paramAttribute = line_stipple.Type.GetAttributeInfo("param");

            line_width.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "line_width");
            line_width.valueAttribute = line_width.Type.GetAttributeInfo("value");
            line_width.paramAttribute = line_width.Type.GetAttributeInfo("param");

            material_ambient.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "material_ambient");
            material_ambient.valueAttribute = material_ambient.Type.GetAttributeInfo("value");
            material_ambient.paramAttribute = material_ambient.Type.GetAttributeInfo("param");

            material_diffuse.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "material_diffuse");
            material_diffuse.valueAttribute = material_diffuse.Type.GetAttributeInfo("value");
            material_diffuse.paramAttribute = material_diffuse.Type.GetAttributeInfo("param");

            material_emission.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "material_emission");
            material_emission.valueAttribute = material_emission.Type.GetAttributeInfo("value");
            material_emission.paramAttribute = material_emission.Type.GetAttributeInfo("param");

            material_shininess.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "material_shininess");
            material_shininess.valueAttribute = material_shininess.Type.GetAttributeInfo("value");
            material_shininess.paramAttribute = material_shininess.Type.GetAttributeInfo("param");

            material_specular.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "material_specular");
            material_specular.valueAttribute = material_specular.Type.GetAttributeInfo("value");
            material_specular.paramAttribute = material_specular.Type.GetAttributeInfo("param");

            model_view_matrix.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "model_view_matrix");
            model_view_matrix.valueAttribute = model_view_matrix.Type.GetAttributeInfo("value");
            model_view_matrix.paramAttribute = model_view_matrix.Type.GetAttributeInfo("param");

            point_distance_attenuation.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "point_distance_attenuation");
            point_distance_attenuation.valueAttribute = point_distance_attenuation.Type.GetAttributeInfo("value");
            point_distance_attenuation.paramAttribute = point_distance_attenuation.Type.GetAttributeInfo("param");

            point_fade_threshold_size.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "point_fade_threshold_size");
            point_fade_threshold_size.valueAttribute = point_fade_threshold_size.Type.GetAttributeInfo("value");
            point_fade_threshold_size.paramAttribute = point_fade_threshold_size.Type.GetAttributeInfo("param");

            point_size.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "point_size");
            point_size.valueAttribute = point_size.Type.GetAttributeInfo("value");
            point_size.paramAttribute = point_size.Type.GetAttributeInfo("param");

            point_size_min.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "point_size_min");
            point_size_min.valueAttribute = point_size_min.Type.GetAttributeInfo("value");
            point_size_min.paramAttribute = point_size_min.Type.GetAttributeInfo("param");

            point_size_max.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "point_size_max");
            point_size_max.valueAttribute = point_size_max.Type.GetAttributeInfo("value");
            point_size_max.paramAttribute = point_size_max.Type.GetAttributeInfo("param");

            polygon_offset.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "polygon_offset");
            polygon_offset.valueAttribute = polygon_offset.Type.GetAttributeInfo("value");
            polygon_offset.paramAttribute = polygon_offset.Type.GetAttributeInfo("param");

            projection_matrix.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "projection_matrix");
            projection_matrix.valueAttribute = projection_matrix.Type.GetAttributeInfo("value");
            projection_matrix.paramAttribute = projection_matrix.Type.GetAttributeInfo("param");

            scissor.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "scissor");
            scissor.valueAttribute = scissor.Type.GetAttributeInfo("value");
            scissor.paramAttribute = scissor.Type.GetAttributeInfo("param");

            stencil_mask.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "stencil_mask");
            stencil_mask.valueAttribute = stencil_mask.Type.GetAttributeInfo("value");
            stencil_mask.paramAttribute = stencil_mask.Type.GetAttributeInfo("param");

            alpha_test_enable.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "alpha_test_enable");
            alpha_test_enable.valueAttribute = alpha_test_enable.Type.GetAttributeInfo("value");
            alpha_test_enable.paramAttribute = alpha_test_enable.Type.GetAttributeInfo("param");

            auto_normal_enable.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "auto_normal_enable");
            auto_normal_enable.valueAttribute = auto_normal_enable.Type.GetAttributeInfo("value");
            auto_normal_enable.paramAttribute = auto_normal_enable.Type.GetAttributeInfo("param");

            blend_enable.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "blend_enable");
            blend_enable.valueAttribute = blend_enable.Type.GetAttributeInfo("value");
            blend_enable.paramAttribute = blend_enable.Type.GetAttributeInfo("param");

            color_logic_op_enable.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "color_logic_op_enable");
            color_logic_op_enable.valueAttribute = color_logic_op_enable.Type.GetAttributeInfo("value");
            color_logic_op_enable.paramAttribute = color_logic_op_enable.Type.GetAttributeInfo("param");

            color_material_enable.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "color_material_enable");
            color_material_enable.valueAttribute = color_material_enable.Type.GetAttributeInfo("value");
            color_material_enable.paramAttribute = color_material_enable.Type.GetAttributeInfo("param");

            cull_face_enable.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "cull_face_enable");
            cull_face_enable.valueAttribute = cull_face_enable.Type.GetAttributeInfo("value");
            cull_face_enable.paramAttribute = cull_face_enable.Type.GetAttributeInfo("param");

            depth_bounds_enable.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "depth_bounds_enable");
            depth_bounds_enable.valueAttribute = depth_bounds_enable.Type.GetAttributeInfo("value");
            depth_bounds_enable.paramAttribute = depth_bounds_enable.Type.GetAttributeInfo("param");

            depth_clamp_enable.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "depth_clamp_enable");
            depth_clamp_enable.valueAttribute = depth_clamp_enable.Type.GetAttributeInfo("value");
            depth_clamp_enable.paramAttribute = depth_clamp_enable.Type.GetAttributeInfo("param");

            depth_test_enable.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "depth_test_enable");
            depth_test_enable.valueAttribute = depth_test_enable.Type.GetAttributeInfo("value");
            depth_test_enable.paramAttribute = depth_test_enable.Type.GetAttributeInfo("param");

            dither_enable.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "dither_enable");
            dither_enable.valueAttribute = dither_enable.Type.GetAttributeInfo("value");
            dither_enable.paramAttribute = dither_enable.Type.GetAttributeInfo("param");

            fog_enable.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "fog_enable");
            fog_enable.valueAttribute = fog_enable.Type.GetAttributeInfo("value");
            fog_enable.paramAttribute = fog_enable.Type.GetAttributeInfo("param");

            light_model_local_viewer_enable.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "light_model_local_viewer_enable");
            light_model_local_viewer_enable.valueAttribute = light_model_local_viewer_enable.Type.GetAttributeInfo("value");
            light_model_local_viewer_enable.paramAttribute = light_model_local_viewer_enable.Type.GetAttributeInfo("param");

            light_model_two_side_enable.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "light_model_two_side_enable");
            light_model_two_side_enable.valueAttribute = light_model_two_side_enable.Type.GetAttributeInfo("value");
            light_model_two_side_enable.paramAttribute = light_model_two_side_enable.Type.GetAttributeInfo("param");

            line_smooth_enable.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "line_smooth_enable");
            line_smooth_enable.valueAttribute = line_smooth_enable.Type.GetAttributeInfo("value");
            line_smooth_enable.paramAttribute = line_smooth_enable.Type.GetAttributeInfo("param");

            line_stipple_enable.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "line_stipple_enable");
            line_stipple_enable.valueAttribute = line_stipple_enable.Type.GetAttributeInfo("value");
            line_stipple_enable.paramAttribute = line_stipple_enable.Type.GetAttributeInfo("param");

            logic_op_enable.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "logic_op_enable");
            logic_op_enable.valueAttribute = logic_op_enable.Type.GetAttributeInfo("value");
            logic_op_enable.paramAttribute = logic_op_enable.Type.GetAttributeInfo("param");

            multisample_enable.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "multisample_enable");
            multisample_enable.valueAttribute = multisample_enable.Type.GetAttributeInfo("value");
            multisample_enable.paramAttribute = multisample_enable.Type.GetAttributeInfo("param");

            normalize_enable.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "normalize_enable");
            normalize_enable.valueAttribute = normalize_enable.Type.GetAttributeInfo("value");
            normalize_enable.paramAttribute = normalize_enable.Type.GetAttributeInfo("param");

            point_smooth_enable.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "point_smooth_enable");
            point_smooth_enable.valueAttribute = point_smooth_enable.Type.GetAttributeInfo("value");
            point_smooth_enable.paramAttribute = point_smooth_enable.Type.GetAttributeInfo("param");

            polygon_offset_fill_enable.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "polygon_offset_fill_enable");
            polygon_offset_fill_enable.valueAttribute = polygon_offset_fill_enable.Type.GetAttributeInfo("value");
            polygon_offset_fill_enable.paramAttribute = polygon_offset_fill_enable.Type.GetAttributeInfo("param");

            polygon_offset_line_enable.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "polygon_offset_line_enable");
            polygon_offset_line_enable.valueAttribute = polygon_offset_line_enable.Type.GetAttributeInfo("value");
            polygon_offset_line_enable.paramAttribute = polygon_offset_line_enable.Type.GetAttributeInfo("param");

            polygon_offset_point_enable.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "polygon_offset_point_enable");
            polygon_offset_point_enable.valueAttribute = polygon_offset_point_enable.Type.GetAttributeInfo("value");
            polygon_offset_point_enable.paramAttribute = polygon_offset_point_enable.Type.GetAttributeInfo("param");

            polygon_smooth_enable.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "polygon_smooth_enable");
            polygon_smooth_enable.valueAttribute = polygon_smooth_enable.Type.GetAttributeInfo("value");
            polygon_smooth_enable.paramAttribute = polygon_smooth_enable.Type.GetAttributeInfo("param");

            polygon_stipple_enable.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "polygon_stipple_enable");
            polygon_stipple_enable.valueAttribute = polygon_stipple_enable.Type.GetAttributeInfo("value");
            polygon_stipple_enable.paramAttribute = polygon_stipple_enable.Type.GetAttributeInfo("param");

            rescale_normal_enable.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "rescale_normal_enable");
            rescale_normal_enable.valueAttribute = rescale_normal_enable.Type.GetAttributeInfo("value");
            rescale_normal_enable.paramAttribute = rescale_normal_enable.Type.GetAttributeInfo("param");

            sample_alpha_to_coverage_enable.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "sample_alpha_to_coverage_enable");
            sample_alpha_to_coverage_enable.valueAttribute = sample_alpha_to_coverage_enable.Type.GetAttributeInfo("value");
            sample_alpha_to_coverage_enable.paramAttribute = sample_alpha_to_coverage_enable.Type.GetAttributeInfo("param");

            sample_alpha_to_one_enable.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "sample_alpha_to_one_enable");
            sample_alpha_to_one_enable.valueAttribute = sample_alpha_to_one_enable.Type.GetAttributeInfo("value");
            sample_alpha_to_one_enable.paramAttribute = sample_alpha_to_one_enable.Type.GetAttributeInfo("param");

            sample_coverage_enable.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "sample_coverage_enable");
            sample_coverage_enable.valueAttribute = sample_coverage_enable.Type.GetAttributeInfo("value");
            sample_coverage_enable.paramAttribute = sample_coverage_enable.Type.GetAttributeInfo("param");

            scissor_test_enable.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "scissor_test_enable");
            scissor_test_enable.valueAttribute = scissor_test_enable.Type.GetAttributeInfo("value");
            scissor_test_enable.paramAttribute = scissor_test_enable.Type.GetAttributeInfo("param");

            stencil_test_enable.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "stencil_test_enable");
            stencil_test_enable.valueAttribute = stencil_test_enable.Type.GetAttributeInfo("value");
            stencil_test_enable.paramAttribute = stencil_test_enable.Type.GetAttributeInfo("param");

            pass_shader.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "pass_shader");
            pass_shader.compiler_optionsAttribute = pass_shader.Type.GetAttributeInfo("compiler_options");
            pass_shader.stageAttribute = pass_shader.Type.GetAttributeInfo("stage");
            pass_shader.annotateChild = pass_shader.Type.GetChildInfo("annotate");
            pass_shader.compiler_targetChild = pass_shader.Type.GetChildInfo("compiler_target");
            pass_shader.nameChild = pass_shader.Type.GetChildInfo("name");
            pass_shader.bindChild = pass_shader.Type.GetChildInfo("bind");

            shader_compiler_target.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "shader_compiler_target");
            shader_compiler_target.Attribute = shader_compiler_target.Type.GetAttributeInfo("");

            shader_name.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "shader_name");
            shader_name.Attribute = shader_name.Type.GetAttributeInfo("");
            shader_name.sourceAttribute = shader_name.Type.GetAttributeInfo("source");

            shader_bind.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "shader_bind");
            shader_bind.boolAttribute = shader_bind.Type.GetAttributeInfo("bool");
            shader_bind.bool1Attribute = shader_bind.Type.GetAttributeInfo("bool1");
            shader_bind.bool2Attribute = shader_bind.Type.GetAttributeInfo("bool2");
            shader_bind.bool3Attribute = shader_bind.Type.GetAttributeInfo("bool3");
            shader_bind.bool4Attribute = shader_bind.Type.GetAttributeInfo("bool4");
            shader_bind.bool1x1Attribute = shader_bind.Type.GetAttributeInfo("bool1x1");
            shader_bind.bool1x2Attribute = shader_bind.Type.GetAttributeInfo("bool1x2");
            shader_bind.bool1x3Attribute = shader_bind.Type.GetAttributeInfo("bool1x3");
            shader_bind.bool1x4Attribute = shader_bind.Type.GetAttributeInfo("bool1x4");
            shader_bind.bool2x1Attribute = shader_bind.Type.GetAttributeInfo("bool2x1");
            shader_bind.bool2x2Attribute = shader_bind.Type.GetAttributeInfo("bool2x2");
            shader_bind.bool2x3Attribute = shader_bind.Type.GetAttributeInfo("bool2x3");
            shader_bind.bool2x4Attribute = shader_bind.Type.GetAttributeInfo("bool2x4");
            shader_bind.bool3x1Attribute = shader_bind.Type.GetAttributeInfo("bool3x1");
            shader_bind.bool3x2Attribute = shader_bind.Type.GetAttributeInfo("bool3x2");
            shader_bind.bool3x3Attribute = shader_bind.Type.GetAttributeInfo("bool3x3");
            shader_bind.bool3x4Attribute = shader_bind.Type.GetAttributeInfo("bool3x4");
            shader_bind.bool4x1Attribute = shader_bind.Type.GetAttributeInfo("bool4x1");
            shader_bind.bool4x2Attribute = shader_bind.Type.GetAttributeInfo("bool4x2");
            shader_bind.bool4x3Attribute = shader_bind.Type.GetAttributeInfo("bool4x3");
            shader_bind.bool4x4Attribute = shader_bind.Type.GetAttributeInfo("bool4x4");
            shader_bind.floatAttribute = shader_bind.Type.GetAttributeInfo("float");
            shader_bind.float1Attribute = shader_bind.Type.GetAttributeInfo("float1");
            shader_bind.float2Attribute = shader_bind.Type.GetAttributeInfo("float2");
            shader_bind.float3Attribute = shader_bind.Type.GetAttributeInfo("float3");
            shader_bind.float4Attribute = shader_bind.Type.GetAttributeInfo("float4");
            shader_bind.float1x1Attribute = shader_bind.Type.GetAttributeInfo("float1x1");
            shader_bind.float1x2Attribute = shader_bind.Type.GetAttributeInfo("float1x2");
            shader_bind.float1x3Attribute = shader_bind.Type.GetAttributeInfo("float1x3");
            shader_bind.float1x4Attribute = shader_bind.Type.GetAttributeInfo("float1x4");
            shader_bind.float2x1Attribute = shader_bind.Type.GetAttributeInfo("float2x1");
            shader_bind.float2x2Attribute = shader_bind.Type.GetAttributeInfo("float2x2");
            shader_bind.float2x3Attribute = shader_bind.Type.GetAttributeInfo("float2x3");
            shader_bind.float2x4Attribute = shader_bind.Type.GetAttributeInfo("float2x4");
            shader_bind.float3x1Attribute = shader_bind.Type.GetAttributeInfo("float3x1");
            shader_bind.float3x2Attribute = shader_bind.Type.GetAttributeInfo("float3x2");
            shader_bind.float3x3Attribute = shader_bind.Type.GetAttributeInfo("float3x3");
            shader_bind.float3x4Attribute = shader_bind.Type.GetAttributeInfo("float3x4");
            shader_bind.float4x1Attribute = shader_bind.Type.GetAttributeInfo("float4x1");
            shader_bind.float4x2Attribute = shader_bind.Type.GetAttributeInfo("float4x2");
            shader_bind.float4x3Attribute = shader_bind.Type.GetAttributeInfo("float4x3");
            shader_bind.float4x4Attribute = shader_bind.Type.GetAttributeInfo("float4x4");
            shader_bind.intAttribute = shader_bind.Type.GetAttributeInfo("int");
            shader_bind.int1Attribute = shader_bind.Type.GetAttributeInfo("int1");
            shader_bind.int2Attribute = shader_bind.Type.GetAttributeInfo("int2");
            shader_bind.int3Attribute = shader_bind.Type.GetAttributeInfo("int3");
            shader_bind.int4Attribute = shader_bind.Type.GetAttributeInfo("int4");
            shader_bind.int1x1Attribute = shader_bind.Type.GetAttributeInfo("int1x1");
            shader_bind.int1x2Attribute = shader_bind.Type.GetAttributeInfo("int1x2");
            shader_bind.int1x3Attribute = shader_bind.Type.GetAttributeInfo("int1x3");
            shader_bind.int1x4Attribute = shader_bind.Type.GetAttributeInfo("int1x4");
            shader_bind.int2x1Attribute = shader_bind.Type.GetAttributeInfo("int2x1");
            shader_bind.int2x2Attribute = shader_bind.Type.GetAttributeInfo("int2x2");
            shader_bind.int2x3Attribute = shader_bind.Type.GetAttributeInfo("int2x3");
            shader_bind.int2x4Attribute = shader_bind.Type.GetAttributeInfo("int2x4");
            shader_bind.int3x1Attribute = shader_bind.Type.GetAttributeInfo("int3x1");
            shader_bind.int3x2Attribute = shader_bind.Type.GetAttributeInfo("int3x2");
            shader_bind.int3x3Attribute = shader_bind.Type.GetAttributeInfo("int3x3");
            shader_bind.int3x4Attribute = shader_bind.Type.GetAttributeInfo("int3x4");
            shader_bind.int4x1Attribute = shader_bind.Type.GetAttributeInfo("int4x1");
            shader_bind.int4x2Attribute = shader_bind.Type.GetAttributeInfo("int4x2");
            shader_bind.int4x3Attribute = shader_bind.Type.GetAttributeInfo("int4x3");
            shader_bind.int4x4Attribute = shader_bind.Type.GetAttributeInfo("int4x4");
            shader_bind.halfAttribute = shader_bind.Type.GetAttributeInfo("half");
            shader_bind.half1Attribute = shader_bind.Type.GetAttributeInfo("half1");
            shader_bind.half2Attribute = shader_bind.Type.GetAttributeInfo("half2");
            shader_bind.half3Attribute = shader_bind.Type.GetAttributeInfo("half3");
            shader_bind.half4Attribute = shader_bind.Type.GetAttributeInfo("half4");
            shader_bind.half1x1Attribute = shader_bind.Type.GetAttributeInfo("half1x1");
            shader_bind.half1x2Attribute = shader_bind.Type.GetAttributeInfo("half1x2");
            shader_bind.half1x3Attribute = shader_bind.Type.GetAttributeInfo("half1x3");
            shader_bind.half1x4Attribute = shader_bind.Type.GetAttributeInfo("half1x4");
            shader_bind.half2x1Attribute = shader_bind.Type.GetAttributeInfo("half2x1");
            shader_bind.half2x2Attribute = shader_bind.Type.GetAttributeInfo("half2x2");
            shader_bind.half2x3Attribute = shader_bind.Type.GetAttributeInfo("half2x3");
            shader_bind.half2x4Attribute = shader_bind.Type.GetAttributeInfo("half2x4");
            shader_bind.half3x1Attribute = shader_bind.Type.GetAttributeInfo("half3x1");
            shader_bind.half3x2Attribute = shader_bind.Type.GetAttributeInfo("half3x2");
            shader_bind.half3x3Attribute = shader_bind.Type.GetAttributeInfo("half3x3");
            shader_bind.half3x4Attribute = shader_bind.Type.GetAttributeInfo("half3x4");
            shader_bind.half4x1Attribute = shader_bind.Type.GetAttributeInfo("half4x1");
            shader_bind.half4x2Attribute = shader_bind.Type.GetAttributeInfo("half4x2");
            shader_bind.half4x3Attribute = shader_bind.Type.GetAttributeInfo("half4x3");
            shader_bind.half4x4Attribute = shader_bind.Type.GetAttributeInfo("half4x4");
            shader_bind.fixedAttribute = shader_bind.Type.GetAttributeInfo("fixed");
            shader_bind.fixed1Attribute = shader_bind.Type.GetAttributeInfo("fixed1");
            shader_bind.fixed2Attribute = shader_bind.Type.GetAttributeInfo("fixed2");
            shader_bind.fixed3Attribute = shader_bind.Type.GetAttributeInfo("fixed3");
            shader_bind.fixed4Attribute = shader_bind.Type.GetAttributeInfo("fixed4");
            shader_bind.fixed1x1Attribute = shader_bind.Type.GetAttributeInfo("fixed1x1");
            shader_bind.fixed1x2Attribute = shader_bind.Type.GetAttributeInfo("fixed1x2");
            shader_bind.fixed1x3Attribute = shader_bind.Type.GetAttributeInfo("fixed1x3");
            shader_bind.fixed1x4Attribute = shader_bind.Type.GetAttributeInfo("fixed1x4");
            shader_bind.fixed2x1Attribute = shader_bind.Type.GetAttributeInfo("fixed2x1");
            shader_bind.fixed2x2Attribute = shader_bind.Type.GetAttributeInfo("fixed2x2");
            shader_bind.fixed2x3Attribute = shader_bind.Type.GetAttributeInfo("fixed2x3");
            shader_bind.fixed2x4Attribute = shader_bind.Type.GetAttributeInfo("fixed2x4");
            shader_bind.fixed3x1Attribute = shader_bind.Type.GetAttributeInfo("fixed3x1");
            shader_bind.fixed3x2Attribute = shader_bind.Type.GetAttributeInfo("fixed3x2");
            shader_bind.fixed3x3Attribute = shader_bind.Type.GetAttributeInfo("fixed3x3");
            shader_bind.fixed3x4Attribute = shader_bind.Type.GetAttributeInfo("fixed3x4");
            shader_bind.fixed4x1Attribute = shader_bind.Type.GetAttributeInfo("fixed4x1");
            shader_bind.fixed4x2Attribute = shader_bind.Type.GetAttributeInfo("fixed4x2");
            shader_bind.fixed4x3Attribute = shader_bind.Type.GetAttributeInfo("fixed4x3");
            shader_bind.fixed4x4Attribute = shader_bind.Type.GetAttributeInfo("fixed4x4");
            shader_bind.stringAttribute = shader_bind.Type.GetAttributeInfo("string");
            shader_bind.enumAttribute = shader_bind.Type.GetAttributeInfo("enum");
            shader_bind.symbolAttribute = shader_bind.Type.GetAttributeInfo("symbol");
            shader_bind.surfaceChild = shader_bind.Type.GetChildInfo("surface");
            shader_bind.sampler1DChild = shader_bind.Type.GetChildInfo("sampler1D");
            shader_bind.sampler2DChild = shader_bind.Type.GetChildInfo("sampler2D");
            shader_bind.sampler3DChild = shader_bind.Type.GetChildInfo("sampler3D");
            shader_bind.samplerRECTChild = shader_bind.Type.GetChildInfo("samplerRECT");
            shader_bind.samplerCUBEChild = shader_bind.Type.GetChildInfo("samplerCUBE");
            shader_bind.samplerDEPTHChild = shader_bind.Type.GetChildInfo("samplerDEPTH");
            shader_bind.paramChild = shader_bind.Type.GetChildInfo("param");

            bind_param.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "bind_param");
            bind_param.refAttribute = bind_param.Type.GetAttributeInfo("ref");

            profile_COMMON.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "profile_COMMON");
            profile_COMMON.idAttribute = profile_COMMON.Type.GetAttributeInfo("id");
            profile_COMMON.assetChild = profile_COMMON.Type.GetChildInfo("asset");
            profile_COMMON.imageChild = profile_COMMON.Type.GetChildInfo("image");
            profile_COMMON.newparamChild = profile_COMMON.Type.GetChildInfo("newparam");
            profile_COMMON.techniqueChild = profile_COMMON.Type.GetChildInfo("technique");
            profile_COMMON.extraChild = profile_COMMON.Type.GetChildInfo("extra");

            common_newparam_type.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "common_newparam_type");
            common_newparam_type.semanticAttribute = common_newparam_type.Type.GetAttributeInfo("semantic");
            common_newparam_type.floatAttribute = common_newparam_type.Type.GetAttributeInfo("float");
            common_newparam_type.float2Attribute = common_newparam_type.Type.GetAttributeInfo("float2");
            common_newparam_type.float3Attribute = common_newparam_type.Type.GetAttributeInfo("float3");
            common_newparam_type.float4Attribute = common_newparam_type.Type.GetAttributeInfo("float4");
            common_newparam_type.sidAttribute = common_newparam_type.Type.GetAttributeInfo("sid");
            common_newparam_type.surfaceChild = common_newparam_type.Type.GetChildInfo("surface");
            common_newparam_type.sampler2DChild = common_newparam_type.Type.GetChildInfo("sampler2D");

            profile_COMMON_technique.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "profile_COMMON_technique");
            profile_COMMON_technique.idAttribute = profile_COMMON_technique.Type.GetAttributeInfo("id");
            profile_COMMON_technique.sidAttribute = profile_COMMON_technique.Type.GetAttributeInfo("sid");
            profile_COMMON_technique.assetChild = profile_COMMON_technique.Type.GetChildInfo("asset");
            profile_COMMON_technique.imageChild = profile_COMMON_technique.Type.GetChildInfo("image");
            profile_COMMON_technique.newparamChild = profile_COMMON_technique.Type.GetChildInfo("newparam");
            profile_COMMON_technique.constantChild = profile_COMMON_technique.Type.GetChildInfo("constant");
            profile_COMMON_technique.lambertChild = profile_COMMON_technique.Type.GetChildInfo("lambert");
            profile_COMMON_technique.phongChild = profile_COMMON_technique.Type.GetChildInfo("phong");
            profile_COMMON_technique.blinnChild = profile_COMMON_technique.Type.GetChildInfo("blinn");
            profile_COMMON_technique.extraChild = profile_COMMON_technique.Type.GetChildInfo("extra");

            technique_constant.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "technique_constant");
            technique_constant.emissionChild = technique_constant.Type.GetChildInfo("emission");
            technique_constant.reflectiveChild = technique_constant.Type.GetChildInfo("reflective");
            technique_constant.reflectivityChild = technique_constant.Type.GetChildInfo("reflectivity");
            technique_constant.transparentChild = technique_constant.Type.GetChildInfo("transparent");
            technique_constant.transparencyChild = technique_constant.Type.GetChildInfo("transparency");
            technique_constant.index_of_refractionChild = technique_constant.Type.GetChildInfo("index_of_refraction");

            common_color_or_texture_type.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "common_color_or_texture_type");
            common_color_or_texture_type.colorChild = common_color_or_texture_type.Type.GetChildInfo("color");
            common_color_or_texture_type.paramChild = common_color_or_texture_type.Type.GetChildInfo("param");
            common_color_or_texture_type.textureChild = common_color_or_texture_type.Type.GetChildInfo("texture");

            common_color_or_texture_type_color.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "common_color_or_texture_type_color");
            common_color_or_texture_type_color.Attribute = common_color_or_texture_type_color.Type.GetAttributeInfo("");
            common_color_or_texture_type_color.sidAttribute = common_color_or_texture_type_color.Type.GetAttributeInfo("sid");

            common_color_or_texture_type_param.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "common_color_or_texture_type_param");
            common_color_or_texture_type_param.refAttribute = common_color_or_texture_type_param.Type.GetAttributeInfo("ref");

            common_color_or_texture_type_texture.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "common_color_or_texture_type_texture");
            common_color_or_texture_type_texture.textureAttribute = common_color_or_texture_type_texture.Type.GetAttributeInfo("texture");
            common_color_or_texture_type_texture.texcoordAttribute = common_color_or_texture_type_texture.Type.GetAttributeInfo("texcoord");
            common_color_or_texture_type_texture.extraChild = common_color_or_texture_type_texture.Type.GetChildInfo("extra");

            common_float_or_param_type.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "common_float_or_param_type");
            common_float_or_param_type.floatChild = common_float_or_param_type.Type.GetChildInfo("float");
            common_float_or_param_type.paramChild = common_float_or_param_type.Type.GetChildInfo("param");

            common_float_or_param_type_float.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "common_float_or_param_type_float");
            common_float_or_param_type_float.Attribute = common_float_or_param_type_float.Type.GetAttributeInfo("");
            common_float_or_param_type_float.sidAttribute = common_float_or_param_type_float.Type.GetAttributeInfo("sid");

            common_float_or_param_type_param.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "common_float_or_param_type_param");
            common_float_or_param_type_param.refAttribute = common_float_or_param_type_param.Type.GetAttributeInfo("ref");

            common_transparent_type.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "common_transparent_type");
            common_transparent_type.opaqueAttribute = common_transparent_type.Type.GetAttributeInfo("opaque");
            common_transparent_type.colorChild = common_transparent_type.Type.GetChildInfo("color");
            common_transparent_type.paramChild = common_transparent_type.Type.GetChildInfo("param");
            common_transparent_type.textureChild = common_transparent_type.Type.GetChildInfo("texture");

            technique_lambert.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "technique_lambert");
            technique_lambert.emissionChild = technique_lambert.Type.GetChildInfo("emission");
            technique_lambert.ambientChild = technique_lambert.Type.GetChildInfo("ambient");
            technique_lambert.diffuseChild = technique_lambert.Type.GetChildInfo("diffuse");
            technique_lambert.reflectiveChild = technique_lambert.Type.GetChildInfo("reflective");
            technique_lambert.reflectivityChild = technique_lambert.Type.GetChildInfo("reflectivity");
            technique_lambert.transparentChild = technique_lambert.Type.GetChildInfo("transparent");
            technique_lambert.transparencyChild = technique_lambert.Type.GetChildInfo("transparency");
            technique_lambert.index_of_refractionChild = technique_lambert.Type.GetChildInfo("index_of_refraction");

            technique_phong.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "technique_phong");
            technique_phong.emissionChild = technique_phong.Type.GetChildInfo("emission");
            technique_phong.ambientChild = technique_phong.Type.GetChildInfo("ambient");
            technique_phong.diffuseChild = technique_phong.Type.GetChildInfo("diffuse");
            technique_phong.specularChild = technique_phong.Type.GetChildInfo("specular");
            technique_phong.shininessChild = technique_phong.Type.GetChildInfo("shininess");
            technique_phong.reflectiveChild = technique_phong.Type.GetChildInfo("reflective");
            technique_phong.reflectivityChild = technique_phong.Type.GetChildInfo("reflectivity");
            technique_phong.transparentChild = technique_phong.Type.GetChildInfo("transparent");
            technique_phong.transparencyChild = technique_phong.Type.GetChildInfo("transparency");
            technique_phong.index_of_refractionChild = technique_phong.Type.GetChildInfo("index_of_refraction");

            technique_blinn.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "technique_blinn");
            technique_blinn.emissionChild = technique_blinn.Type.GetChildInfo("emission");
            technique_blinn.ambientChild = technique_blinn.Type.GetChildInfo("ambient");
            technique_blinn.diffuseChild = technique_blinn.Type.GetChildInfo("diffuse");
            technique_blinn.specularChild = technique_blinn.Type.GetChildInfo("specular");
            technique_blinn.shininessChild = technique_blinn.Type.GetChildInfo("shininess");
            technique_blinn.reflectiveChild = technique_blinn.Type.GetChildInfo("reflective");
            technique_blinn.reflectivityChild = technique_blinn.Type.GetChildInfo("reflectivity");
            technique_blinn.transparentChild = technique_blinn.Type.GetChildInfo("transparent");
            technique_blinn.transparencyChild = technique_blinn.Type.GetChildInfo("transparency");
            technique_blinn.index_of_refractionChild = technique_blinn.Type.GetChildInfo("index_of_refraction");

            library_force_fields.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "library_force_fields");
            library_force_fields.idAttribute = library_force_fields.Type.GetAttributeInfo("id");
            library_force_fields.nameAttribute = library_force_fields.Type.GetAttributeInfo("name");
            library_force_fields.assetChild = library_force_fields.Type.GetChildInfo("asset");
            library_force_fields.force_fieldChild = library_force_fields.Type.GetChildInfo("force_field");
            library_force_fields.extraChild = library_force_fields.Type.GetChildInfo("extra");

            force_field.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "force_field");
            force_field.idAttribute = force_field.Type.GetAttributeInfo("id");
            force_field.nameAttribute = force_field.Type.GetAttributeInfo("name");
            force_field.assetChild = force_field.Type.GetChildInfo("asset");
            force_field.techniqueChild = force_field.Type.GetChildInfo("technique");
            force_field.extraChild = force_field.Type.GetChildInfo("extra");

            library_images.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "library_images");
            library_images.idAttribute = library_images.Type.GetAttributeInfo("id");
            library_images.nameAttribute = library_images.Type.GetAttributeInfo("name");
            library_images.assetChild = library_images.Type.GetChildInfo("asset");
            library_images.imageChild = library_images.Type.GetChildInfo("image");
            library_images.extraChild = library_images.Type.GetChildInfo("extra");

            library_lights.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "library_lights");
            library_lights.idAttribute = library_lights.Type.GetAttributeInfo("id");
            library_lights.nameAttribute = library_lights.Type.GetAttributeInfo("name");
            library_lights.assetChild = library_lights.Type.GetChildInfo("asset");
            library_lights.lightChild = library_lights.Type.GetChildInfo("light");
            library_lights.extraChild = library_lights.Type.GetChildInfo("extra");

            light.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "light");
            light.idAttribute = light.Type.GetAttributeInfo("id");
            light.nameAttribute = light.Type.GetAttributeInfo("name");
            light.assetChild = light.Type.GetChildInfo("asset");
            light.technique_commonChild = light.Type.GetChildInfo("technique_common");
            light.techniqueChild = light.Type.GetChildInfo("technique");
            light.extraChild = light.Type.GetChildInfo("extra");

            light_technique_common.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "light_technique_common");
            light_technique_common.ambientChild = light_technique_common.Type.GetChildInfo("ambient");
            light_technique_common.directionalChild = light_technique_common.Type.GetChildInfo("directional");
            light_technique_common.pointChild = light_technique_common.Type.GetChildInfo("point");
            light_technique_common.spotChild = light_technique_common.Type.GetChildInfo("spot");

            technique_common_ambient.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "technique_common_ambient");
            technique_common_ambient.colorChild = technique_common_ambient.Type.GetChildInfo("color");

            TargetableFloat3.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "TargetableFloat3");
            TargetableFloat3.Attribute = TargetableFloat3.Type.GetAttributeInfo("");
            TargetableFloat3.sidAttribute = TargetableFloat3.Type.GetAttributeInfo("sid");

            technique_common_directional.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "technique_common_directional");
            technique_common_directional.colorChild = technique_common_directional.Type.GetChildInfo("color");

            technique_common_point.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "technique_common_point");
            technique_common_point.colorChild = technique_common_point.Type.GetChildInfo("color");
            technique_common_point.constant_attenuationChild = technique_common_point.Type.GetChildInfo("constant_attenuation");
            technique_common_point.linear_attenuationChild = technique_common_point.Type.GetChildInfo("linear_attenuation");
            technique_common_point.quadratic_attenuationChild = technique_common_point.Type.GetChildInfo("quadratic_attenuation");

            technique_common_spot.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "technique_common_spot");
            technique_common_spot.colorChild = technique_common_spot.Type.GetChildInfo("color");
            technique_common_spot.constant_attenuationChild = technique_common_spot.Type.GetChildInfo("constant_attenuation");
            technique_common_spot.linear_attenuationChild = technique_common_spot.Type.GetChildInfo("linear_attenuation");
            technique_common_spot.quadratic_attenuationChild = technique_common_spot.Type.GetChildInfo("quadratic_attenuation");
            technique_common_spot.falloff_angleChild = technique_common_spot.Type.GetChildInfo("falloff_angle");
            technique_common_spot.falloff_exponentChild = technique_common_spot.Type.GetChildInfo("falloff_exponent");

            library_materials.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "library_materials");
            library_materials.idAttribute = library_materials.Type.GetAttributeInfo("id");
            library_materials.nameAttribute = library_materials.Type.GetAttributeInfo("name");
            library_materials.assetChild = library_materials.Type.GetChildInfo("asset");
            library_materials.materialChild = library_materials.Type.GetChildInfo("material");
            library_materials.extraChild = library_materials.Type.GetChildInfo("extra");

            material.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "material");
            material.idAttribute = material.Type.GetAttributeInfo("id");
            material.nameAttribute = material.Type.GetAttributeInfo("name");
            material.assetChild = material.Type.GetChildInfo("asset");
            material.instance_effectChild = material.Type.GetChildInfo("instance_effect");
            material.extraChild = material.Type.GetChildInfo("extra");

            instance_effect.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "instance_effect");
            instance_effect.urlAttribute = instance_effect.Type.GetAttributeInfo("url");
            instance_effect.sidAttribute = instance_effect.Type.GetAttributeInfo("sid");
            instance_effect.nameAttribute = instance_effect.Type.GetAttributeInfo("name");
            instance_effect.technique_hintChild = instance_effect.Type.GetChildInfo("technique_hint");
            instance_effect.setparamChild = instance_effect.Type.GetChildInfo("setparam");
            instance_effect.extraChild = instance_effect.Type.GetChildInfo("extra");

            instance_effect_technique_hint.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "instance_effect_technique_hint");
            instance_effect_technique_hint.platformAttribute = instance_effect_technique_hint.Type.GetAttributeInfo("platform");
            instance_effect_technique_hint.profileAttribute = instance_effect_technique_hint.Type.GetAttributeInfo("profile");
            instance_effect_technique_hint.refAttribute = instance_effect_technique_hint.Type.GetAttributeInfo("ref");

            instance_effect_setparam.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "instance_effect_setparam");
            instance_effect_setparam.boolAttribute = instance_effect_setparam.Type.GetAttributeInfo("bool");
            instance_effect_setparam.bool2Attribute = instance_effect_setparam.Type.GetAttributeInfo("bool2");
            instance_effect_setparam.bool3Attribute = instance_effect_setparam.Type.GetAttributeInfo("bool3");
            instance_effect_setparam.bool4Attribute = instance_effect_setparam.Type.GetAttributeInfo("bool4");
            instance_effect_setparam.intAttribute = instance_effect_setparam.Type.GetAttributeInfo("int");
            instance_effect_setparam.int2Attribute = instance_effect_setparam.Type.GetAttributeInfo("int2");
            instance_effect_setparam.int3Attribute = instance_effect_setparam.Type.GetAttributeInfo("int3");
            instance_effect_setparam.int4Attribute = instance_effect_setparam.Type.GetAttributeInfo("int4");
            instance_effect_setparam.floatAttribute = instance_effect_setparam.Type.GetAttributeInfo("float");
            instance_effect_setparam.float2Attribute = instance_effect_setparam.Type.GetAttributeInfo("float2");
            instance_effect_setparam.float3Attribute = instance_effect_setparam.Type.GetAttributeInfo("float3");
            instance_effect_setparam.float4Attribute = instance_effect_setparam.Type.GetAttributeInfo("float4");
            instance_effect_setparam.float1x1Attribute = instance_effect_setparam.Type.GetAttributeInfo("float1x1");
            instance_effect_setparam.float1x2Attribute = instance_effect_setparam.Type.GetAttributeInfo("float1x2");
            instance_effect_setparam.float1x3Attribute = instance_effect_setparam.Type.GetAttributeInfo("float1x3");
            instance_effect_setparam.float1x4Attribute = instance_effect_setparam.Type.GetAttributeInfo("float1x4");
            instance_effect_setparam.float2x1Attribute = instance_effect_setparam.Type.GetAttributeInfo("float2x1");
            instance_effect_setparam.float2x2Attribute = instance_effect_setparam.Type.GetAttributeInfo("float2x2");
            instance_effect_setparam.float2x3Attribute = instance_effect_setparam.Type.GetAttributeInfo("float2x3");
            instance_effect_setparam.float2x4Attribute = instance_effect_setparam.Type.GetAttributeInfo("float2x4");
            instance_effect_setparam.float3x1Attribute = instance_effect_setparam.Type.GetAttributeInfo("float3x1");
            instance_effect_setparam.float3x2Attribute = instance_effect_setparam.Type.GetAttributeInfo("float3x2");
            instance_effect_setparam.float3x3Attribute = instance_effect_setparam.Type.GetAttributeInfo("float3x3");
            instance_effect_setparam.float3x4Attribute = instance_effect_setparam.Type.GetAttributeInfo("float3x4");
            instance_effect_setparam.float4x1Attribute = instance_effect_setparam.Type.GetAttributeInfo("float4x1");
            instance_effect_setparam.float4x2Attribute = instance_effect_setparam.Type.GetAttributeInfo("float4x2");
            instance_effect_setparam.float4x3Attribute = instance_effect_setparam.Type.GetAttributeInfo("float4x3");
            instance_effect_setparam.float4x4Attribute = instance_effect_setparam.Type.GetAttributeInfo("float4x4");
            instance_effect_setparam.enumAttribute = instance_effect_setparam.Type.GetAttributeInfo("enum");
            instance_effect_setparam.refAttribute = instance_effect_setparam.Type.GetAttributeInfo("ref");
            instance_effect_setparam.surfaceChild = instance_effect_setparam.Type.GetChildInfo("surface");
            instance_effect_setparam.sampler1DChild = instance_effect_setparam.Type.GetChildInfo("sampler1D");
            instance_effect_setparam.sampler2DChild = instance_effect_setparam.Type.GetChildInfo("sampler2D");
            instance_effect_setparam.sampler3DChild = instance_effect_setparam.Type.GetChildInfo("sampler3D");
            instance_effect_setparam.samplerCUBEChild = instance_effect_setparam.Type.GetChildInfo("samplerCUBE");
            instance_effect_setparam.samplerRECTChild = instance_effect_setparam.Type.GetChildInfo("samplerRECT");
            instance_effect_setparam.samplerDEPTHChild = instance_effect_setparam.Type.GetChildInfo("samplerDEPTH");

            library_nodes.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "library_nodes");
            library_nodes.idAttribute = library_nodes.Type.GetAttributeInfo("id");
            library_nodes.nameAttribute = library_nodes.Type.GetAttributeInfo("name");
            library_nodes.assetChild = library_nodes.Type.GetChildInfo("asset");
            library_nodes.nodeChild = library_nodes.Type.GetChildInfo("node");
            library_nodes.extraChild = library_nodes.Type.GetChildInfo("extra");

            node.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "node");
            node.idAttribute = node.Type.GetAttributeInfo("id");
            node.nameAttribute = node.Type.GetAttributeInfo("name");
            node.sidAttribute = node.Type.GetAttributeInfo("sid");
            node.typeAttribute = node.Type.GetAttributeInfo("type");
            node.layerAttribute = node.Type.GetAttributeInfo("layer");
            node.assetChild = node.Type.GetChildInfo("asset");
            node.lookatChild = node.Type.GetChildInfo("lookat");
            node.matrixChild = node.Type.GetChildInfo("matrix");
            node.rotateChild = node.Type.GetChildInfo("rotate");
            node.scaleChild = node.Type.GetChildInfo("scale");
            node.skewChild = node.Type.GetChildInfo("skew");
            node.translateChild = node.Type.GetChildInfo("translate");
            node.instance_cameraChild = node.Type.GetChildInfo("instance_camera");
            node.instance_controllerChild = node.Type.GetChildInfo("instance_controller");
            node.instance_geometryChild = node.Type.GetChildInfo("instance_geometry");
            node.instance_lightChild = node.Type.GetChildInfo("instance_light");
            node.instance_nodeChild = node.Type.GetChildInfo("instance_node");
            node.nodeChild = node.Type.GetChildInfo("node");
            node.extraChild = node.Type.GetChildInfo("extra");

            lookat.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "lookat");
            lookat.Attribute = lookat.Type.GetAttributeInfo("");
            lookat.sidAttribute = lookat.Type.GetAttributeInfo("sid");

            matrix.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "matrix");
            matrix.Attribute = matrix.Type.GetAttributeInfo("");
            matrix.sidAttribute = matrix.Type.GetAttributeInfo("sid");

            rotate.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "rotate");
            rotate.Attribute = rotate.Type.GetAttributeInfo("");
            rotate.sidAttribute = rotate.Type.GetAttributeInfo("sid");

            skew.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "skew");
            skew.Attribute = skew.Type.GetAttributeInfo("");
            skew.sidAttribute = skew.Type.GetAttributeInfo("sid");

            instance_controller.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "instance_controller");
            instance_controller.urlAttribute = instance_controller.Type.GetAttributeInfo("url");
            instance_controller.sidAttribute = instance_controller.Type.GetAttributeInfo("sid");
            instance_controller.nameAttribute = instance_controller.Type.GetAttributeInfo("name");
            instance_controller.skeletonChild = instance_controller.Type.GetChildInfo("skeleton");
            instance_controller.bind_materialChild = instance_controller.Type.GetChildInfo("bind_material");
            instance_controller.extraChild = instance_controller.Type.GetChildInfo("extra");

            bind_material.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "bind_material");
            bind_material.paramChild = bind_material.Type.GetChildInfo("param");
            bind_material.technique_commonChild = bind_material.Type.GetChildInfo("technique_common");
            bind_material.techniqueChild = bind_material.Type.GetChildInfo("technique");
            bind_material.extraChild = bind_material.Type.GetChildInfo("extra");

            bind_material_technique_common.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "bind_material_technique_common");
            bind_material_technique_common.instance_materialChild = bind_material_technique_common.Type.GetChildInfo("instance_material");

            instance_material.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "instance_material");
            instance_material.symbolAttribute = instance_material.Type.GetAttributeInfo("symbol");
            instance_material.targetAttribute = instance_material.Type.GetAttributeInfo("target");
            instance_material.sidAttribute = instance_material.Type.GetAttributeInfo("sid");
            instance_material.nameAttribute = instance_material.Type.GetAttributeInfo("name");
            instance_material.bindChild = instance_material.Type.GetChildInfo("bind");
            instance_material.bind_vertex_inputChild = instance_material.Type.GetChildInfo("bind_vertex_input");
            instance_material.extraChild = instance_material.Type.GetChildInfo("extra");

            instance_material_bind.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "instance_material_bind");
            instance_material_bind.semanticAttribute = instance_material_bind.Type.GetAttributeInfo("semantic");
            instance_material_bind.targetAttribute = instance_material_bind.Type.GetAttributeInfo("target");

            instance_material_bind_vertex_input.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "instance_material_bind_vertex_input");
            instance_material_bind_vertex_input.semanticAttribute = instance_material_bind_vertex_input.Type.GetAttributeInfo("semantic");
            instance_material_bind_vertex_input.input_semanticAttribute = instance_material_bind_vertex_input.Type.GetAttributeInfo("input_semantic");
            instance_material_bind_vertex_input.input_setAttribute = instance_material_bind_vertex_input.Type.GetAttributeInfo("input_set");

            instance_geometry.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "instance_geometry");
            instance_geometry.urlAttribute = instance_geometry.Type.GetAttributeInfo("url");
            instance_geometry.sidAttribute = instance_geometry.Type.GetAttributeInfo("sid");
            instance_geometry.nameAttribute = instance_geometry.Type.GetAttributeInfo("name");
            instance_geometry.bind_materialChild = instance_geometry.Type.GetChildInfo("bind_material");
            instance_geometry.extraChild = instance_geometry.Type.GetChildInfo("extra");

            library_physics_materials.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "library_physics_materials");
            library_physics_materials.idAttribute = library_physics_materials.Type.GetAttributeInfo("id");
            library_physics_materials.nameAttribute = library_physics_materials.Type.GetAttributeInfo("name");
            library_physics_materials.assetChild = library_physics_materials.Type.GetChildInfo("asset");
            library_physics_materials.physics_materialChild = library_physics_materials.Type.GetChildInfo("physics_material");
            library_physics_materials.extraChild = library_physics_materials.Type.GetChildInfo("extra");

            physics_material.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "physics_material");
            physics_material.idAttribute = physics_material.Type.GetAttributeInfo("id");
            physics_material.nameAttribute = physics_material.Type.GetAttributeInfo("name");
            physics_material.assetChild = physics_material.Type.GetChildInfo("asset");
            physics_material.technique_commonChild = physics_material.Type.GetChildInfo("technique_common");
            physics_material.techniqueChild = physics_material.Type.GetChildInfo("technique");
            physics_material.extraChild = physics_material.Type.GetChildInfo("extra");

            physics_material_technique_common.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "physics_material_technique_common");
            physics_material_technique_common.dynamic_frictionChild = physics_material_technique_common.Type.GetChildInfo("dynamic_friction");
            physics_material_technique_common.restitutionChild = physics_material_technique_common.Type.GetChildInfo("restitution");
            physics_material_technique_common.static_frictionChild = physics_material_technique_common.Type.GetChildInfo("static_friction");

            library_physics_models.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "library_physics_models");
            library_physics_models.idAttribute = library_physics_models.Type.GetAttributeInfo("id");
            library_physics_models.nameAttribute = library_physics_models.Type.GetAttributeInfo("name");
            library_physics_models.assetChild = library_physics_models.Type.GetChildInfo("asset");
            library_physics_models.physics_modelChild = library_physics_models.Type.GetChildInfo("physics_model");
            library_physics_models.extraChild = library_physics_models.Type.GetChildInfo("extra");

            physics_model.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "physics_model");
            physics_model.idAttribute = physics_model.Type.GetAttributeInfo("id");
            physics_model.nameAttribute = physics_model.Type.GetAttributeInfo("name");
            physics_model.assetChild = physics_model.Type.GetChildInfo("asset");
            physics_model.rigid_bodyChild = physics_model.Type.GetChildInfo("rigid_body");
            physics_model.rigid_constraintChild = physics_model.Type.GetChildInfo("rigid_constraint");
            physics_model.instance_physics_modelChild = physics_model.Type.GetChildInfo("instance_physics_model");
            physics_model.extraChild = physics_model.Type.GetChildInfo("extra");

            rigid_body.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "rigid_body");
            rigid_body.sidAttribute = rigid_body.Type.GetAttributeInfo("sid");
            rigid_body.nameAttribute = rigid_body.Type.GetAttributeInfo("name");
            rigid_body.technique_commonChild = rigid_body.Type.GetChildInfo("technique_common");
            rigid_body.techniqueChild = rigid_body.Type.GetChildInfo("technique");
            rigid_body.extraChild = rigid_body.Type.GetChildInfo("extra");

            rigid_body_technique_common.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "rigid_body_technique_common");
            rigid_body_technique_common.dynamicChild = rigid_body_technique_common.Type.GetChildInfo("dynamic");
            rigid_body_technique_common.massChild = rigid_body_technique_common.Type.GetChildInfo("mass");
            rigid_body_technique_common.mass_frameChild = rigid_body_technique_common.Type.GetChildInfo("mass_frame");
            rigid_body_technique_common.inertiaChild = rigid_body_technique_common.Type.GetChildInfo("inertia");
            rigid_body_technique_common.instance_physics_materialChild = rigid_body_technique_common.Type.GetChildInfo("instance_physics_material");
            rigid_body_technique_common.physics_materialChild = rigid_body_technique_common.Type.GetChildInfo("physics_material");
            rigid_body_technique_common.shapeChild = rigid_body_technique_common.Type.GetChildInfo("shape");

            technique_common_dynamic.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "technique_common_dynamic");
            technique_common_dynamic.Attribute = technique_common_dynamic.Type.GetAttributeInfo("");
            technique_common_dynamic.sidAttribute = technique_common_dynamic.Type.GetAttributeInfo("sid");

            technique_common_mass_frame.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "technique_common_mass_frame");
            technique_common_mass_frame.translateChild = technique_common_mass_frame.Type.GetChildInfo("translate");
            technique_common_mass_frame.rotateChild = technique_common_mass_frame.Type.GetChildInfo("rotate");

            technique_common_shape.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "technique_common_shape");
            technique_common_shape.hollowChild = technique_common_shape.Type.GetChildInfo("hollow");
            technique_common_shape.massChild = technique_common_shape.Type.GetChildInfo("mass");
            technique_common_shape.densityChild = technique_common_shape.Type.GetChildInfo("density");
            technique_common_shape.instance_physics_materialChild = technique_common_shape.Type.GetChildInfo("instance_physics_material");
            technique_common_shape.physics_materialChild = technique_common_shape.Type.GetChildInfo("physics_material");
            technique_common_shape.instance_geometryChild = technique_common_shape.Type.GetChildInfo("instance_geometry");
            technique_common_shape.planeChild = technique_common_shape.Type.GetChildInfo("plane");
            technique_common_shape.boxChild = technique_common_shape.Type.GetChildInfo("box");
            technique_common_shape.sphereChild = technique_common_shape.Type.GetChildInfo("sphere");
            technique_common_shape.cylinderChild = technique_common_shape.Type.GetChildInfo("cylinder");
            technique_common_shape.tapered_cylinderChild = technique_common_shape.Type.GetChildInfo("tapered_cylinder");
            technique_common_shape.capsuleChild = technique_common_shape.Type.GetChildInfo("capsule");
            technique_common_shape.tapered_capsuleChild = technique_common_shape.Type.GetChildInfo("tapered_capsule");
            technique_common_shape.translateChild = technique_common_shape.Type.GetChildInfo("translate");
            technique_common_shape.rotateChild = technique_common_shape.Type.GetChildInfo("rotate");
            technique_common_shape.extraChild = technique_common_shape.Type.GetChildInfo("extra");

            shape_hollow.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "shape_hollow");
            shape_hollow.Attribute = shape_hollow.Type.GetAttributeInfo("");
            shape_hollow.sidAttribute = shape_hollow.Type.GetAttributeInfo("sid");

            plane.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "plane");
            plane.equationAttribute = plane.Type.GetAttributeInfo("equation");
            plane.extraChild = plane.Type.GetChildInfo("extra");

            box.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "box");
            box.half_extentsAttribute = box.Type.GetAttributeInfo("half_extents");
            box.extraChild = box.Type.GetChildInfo("extra");

            sphere.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "sphere");
            sphere.radiusAttribute = sphere.Type.GetAttributeInfo("radius");
            sphere.extraChild = sphere.Type.GetChildInfo("extra");

            cylinder.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "cylinder");
            cylinder.heightAttribute = cylinder.Type.GetAttributeInfo("height");
            cylinder.radiusAttribute = cylinder.Type.GetAttributeInfo("radius");
            cylinder.extraChild = cylinder.Type.GetChildInfo("extra");

            tapered_cylinder.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "tapered_cylinder");
            tapered_cylinder.heightAttribute = tapered_cylinder.Type.GetAttributeInfo("height");
            tapered_cylinder.radius1Attribute = tapered_cylinder.Type.GetAttributeInfo("radius1");
            tapered_cylinder.radius2Attribute = tapered_cylinder.Type.GetAttributeInfo("radius2");
            tapered_cylinder.extraChild = tapered_cylinder.Type.GetChildInfo("extra");

            capsule.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "capsule");
            capsule.heightAttribute = capsule.Type.GetAttributeInfo("height");
            capsule.radiusAttribute = capsule.Type.GetAttributeInfo("radius");
            capsule.extraChild = capsule.Type.GetChildInfo("extra");

            tapered_capsule.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "tapered_capsule");
            tapered_capsule.heightAttribute = tapered_capsule.Type.GetAttributeInfo("height");
            tapered_capsule.radius1Attribute = tapered_capsule.Type.GetAttributeInfo("radius1");
            tapered_capsule.radius2Attribute = tapered_capsule.Type.GetAttributeInfo("radius2");
            tapered_capsule.extraChild = tapered_capsule.Type.GetChildInfo("extra");

            rigid_constraint.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "rigid_constraint");
            rigid_constraint.sidAttribute = rigid_constraint.Type.GetAttributeInfo("sid");
            rigid_constraint.nameAttribute = rigid_constraint.Type.GetAttributeInfo("name");
            rigid_constraint.ref_attachmentChild = rigid_constraint.Type.GetChildInfo("ref_attachment");
            rigid_constraint.attachmentChild = rigid_constraint.Type.GetChildInfo("attachment");
            rigid_constraint.technique_commonChild = rigid_constraint.Type.GetChildInfo("technique_common");
            rigid_constraint.techniqueChild = rigid_constraint.Type.GetChildInfo("technique");
            rigid_constraint.extraChild = rigid_constraint.Type.GetChildInfo("extra");

            rigid_constraint_ref_attachment.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "rigid_constraint_ref_attachment");
            rigid_constraint_ref_attachment.rigid_bodyAttribute = rigid_constraint_ref_attachment.Type.GetAttributeInfo("rigid_body");
            rigid_constraint_ref_attachment.translateChild = rigid_constraint_ref_attachment.Type.GetChildInfo("translate");
            rigid_constraint_ref_attachment.rotateChild = rigid_constraint_ref_attachment.Type.GetChildInfo("rotate");
            rigid_constraint_ref_attachment.extraChild = rigid_constraint_ref_attachment.Type.GetChildInfo("extra");

            rigid_constraint_attachment.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "rigid_constraint_attachment");
            rigid_constraint_attachment.rigid_bodyAttribute = rigid_constraint_attachment.Type.GetAttributeInfo("rigid_body");
            rigid_constraint_attachment.translateChild = rigid_constraint_attachment.Type.GetChildInfo("translate");
            rigid_constraint_attachment.rotateChild = rigid_constraint_attachment.Type.GetChildInfo("rotate");
            rigid_constraint_attachment.extraChild = rigid_constraint_attachment.Type.GetChildInfo("extra");

            rigid_constraint_technique_common.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "rigid_constraint_technique_common");
            rigid_constraint_technique_common.enabledChild = rigid_constraint_technique_common.Type.GetChildInfo("enabled");
            rigid_constraint_technique_common.interpenetrateChild = rigid_constraint_technique_common.Type.GetChildInfo("interpenetrate");
            rigid_constraint_technique_common.limitsChild = rigid_constraint_technique_common.Type.GetChildInfo("limits");
            rigid_constraint_technique_common.springChild = rigid_constraint_technique_common.Type.GetChildInfo("spring");

            technique_common_enabled.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "technique_common_enabled");
            technique_common_enabled.Attribute = technique_common_enabled.Type.GetAttributeInfo("");
            technique_common_enabled.sidAttribute = technique_common_enabled.Type.GetAttributeInfo("sid");

            technique_common_interpenetrate.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "technique_common_interpenetrate");
            technique_common_interpenetrate.Attribute = technique_common_interpenetrate.Type.GetAttributeInfo("");
            technique_common_interpenetrate.sidAttribute = technique_common_interpenetrate.Type.GetAttributeInfo("sid");

            technique_common_limits.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "technique_common_limits");
            technique_common_limits.swing_cone_and_twistChild = technique_common_limits.Type.GetChildInfo("swing_cone_and_twist");
            technique_common_limits.linearChild = technique_common_limits.Type.GetChildInfo("linear");

            limits_swing_cone_and_twist.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "limits_swing_cone_and_twist");
            limits_swing_cone_and_twist.minChild = limits_swing_cone_and_twist.Type.GetChildInfo("min");
            limits_swing_cone_and_twist.maxChild = limits_swing_cone_and_twist.Type.GetChildInfo("max");

            limits_linear.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "limits_linear");
            limits_linear.minChild = limits_linear.Type.GetChildInfo("min");
            limits_linear.maxChild = limits_linear.Type.GetChildInfo("max");

            technique_common_spring.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "technique_common_spring");
            technique_common_spring.angularChild = technique_common_spring.Type.GetChildInfo("angular");
            technique_common_spring.linearChild = technique_common_spring.Type.GetChildInfo("linear");

            spring_angular.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "spring_angular");
            spring_angular.stiffnessChild = spring_angular.Type.GetChildInfo("stiffness");
            spring_angular.dampingChild = spring_angular.Type.GetChildInfo("damping");
            spring_angular.target_valueChild = spring_angular.Type.GetChildInfo("target_value");

            spring_linear.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "spring_linear");
            spring_linear.stiffnessChild = spring_linear.Type.GetChildInfo("stiffness");
            spring_linear.dampingChild = spring_linear.Type.GetChildInfo("damping");
            spring_linear.target_valueChild = spring_linear.Type.GetChildInfo("target_value");

            instance_physics_model.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "instance_physics_model");
            instance_physics_model.urlAttribute = instance_physics_model.Type.GetAttributeInfo("url");
            instance_physics_model.sidAttribute = instance_physics_model.Type.GetAttributeInfo("sid");
            instance_physics_model.nameAttribute = instance_physics_model.Type.GetAttributeInfo("name");
            instance_physics_model.parentAttribute = instance_physics_model.Type.GetAttributeInfo("parent");
            instance_physics_model.instance_force_fieldChild = instance_physics_model.Type.GetChildInfo("instance_force_field");
            instance_physics_model.instance_rigid_bodyChild = instance_physics_model.Type.GetChildInfo("instance_rigid_body");
            instance_physics_model.instance_rigid_constraintChild = instance_physics_model.Type.GetChildInfo("instance_rigid_constraint");
            instance_physics_model.extraChild = instance_physics_model.Type.GetChildInfo("extra");

            instance_rigid_body.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "instance_rigid_body");
            instance_rigid_body.bodyAttribute = instance_rigid_body.Type.GetAttributeInfo("body");
            instance_rigid_body.sidAttribute = instance_rigid_body.Type.GetAttributeInfo("sid");
            instance_rigid_body.nameAttribute = instance_rigid_body.Type.GetAttributeInfo("name");
            instance_rigid_body.targetAttribute = instance_rigid_body.Type.GetAttributeInfo("target");
            instance_rigid_body.technique_commonChild = instance_rigid_body.Type.GetChildInfo("technique_common");
            instance_rigid_body.techniqueChild = instance_rigid_body.Type.GetChildInfo("technique");
            instance_rigid_body.extraChild = instance_rigid_body.Type.GetChildInfo("extra");

            instance_rigid_body_technique_common.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "instance_rigid_body_technique_common");
            instance_rigid_body_technique_common.angular_velocityAttribute = instance_rigid_body_technique_common.Type.GetAttributeInfo("angular_velocity");
            instance_rigid_body_technique_common.velocityAttribute = instance_rigid_body_technique_common.Type.GetAttributeInfo("velocity");
            instance_rigid_body_technique_common.dynamicChild = instance_rigid_body_technique_common.Type.GetChildInfo("dynamic");
            instance_rigid_body_technique_common.massChild = instance_rigid_body_technique_common.Type.GetChildInfo("mass");
            instance_rigid_body_technique_common.mass_frameChild = instance_rigid_body_technique_common.Type.GetChildInfo("mass_frame");
            instance_rigid_body_technique_common.inertiaChild = instance_rigid_body_technique_common.Type.GetChildInfo("inertia");
            instance_rigid_body_technique_common.instance_physics_materialChild = instance_rigid_body_technique_common.Type.GetChildInfo("instance_physics_material");
            instance_rigid_body_technique_common.physics_materialChild = instance_rigid_body_technique_common.Type.GetChildInfo("physics_material");
            instance_rigid_body_technique_common.shapeChild = instance_rigid_body_technique_common.Type.GetChildInfo("shape");

            instance_rigid_body_technique_common_dynamic.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "instance_rigid_body_technique_common_dynamic");
            instance_rigid_body_technique_common_dynamic.Attribute = instance_rigid_body_technique_common_dynamic.Type.GetAttributeInfo("");
            instance_rigid_body_technique_common_dynamic.sidAttribute = instance_rigid_body_technique_common_dynamic.Type.GetAttributeInfo("sid");

            instance_rigid_body_technique_common_mass_frame.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "instance_rigid_body_technique_common_mass_frame");
            instance_rigid_body_technique_common_mass_frame.translateChild = instance_rigid_body_technique_common_mass_frame.Type.GetChildInfo("translate");
            instance_rigid_body_technique_common_mass_frame.rotateChild = instance_rigid_body_technique_common_mass_frame.Type.GetChildInfo("rotate");

            instance_rigid_body_technique_common_shape.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "instance_rigid_body_technique_common_shape");
            instance_rigid_body_technique_common_shape.hollowChild = instance_rigid_body_technique_common_shape.Type.GetChildInfo("hollow");
            instance_rigid_body_technique_common_shape.massChild = instance_rigid_body_technique_common_shape.Type.GetChildInfo("mass");
            instance_rigid_body_technique_common_shape.densityChild = instance_rigid_body_technique_common_shape.Type.GetChildInfo("density");
            instance_rigid_body_technique_common_shape.instance_physics_materialChild = instance_rigid_body_technique_common_shape.Type.GetChildInfo("instance_physics_material");
            instance_rigid_body_technique_common_shape.physics_materialChild = instance_rigid_body_technique_common_shape.Type.GetChildInfo("physics_material");
            instance_rigid_body_technique_common_shape.instance_geometryChild = instance_rigid_body_technique_common_shape.Type.GetChildInfo("instance_geometry");
            instance_rigid_body_technique_common_shape.planeChild = instance_rigid_body_technique_common_shape.Type.GetChildInfo("plane");
            instance_rigid_body_technique_common_shape.boxChild = instance_rigid_body_technique_common_shape.Type.GetChildInfo("box");
            instance_rigid_body_technique_common_shape.sphereChild = instance_rigid_body_technique_common_shape.Type.GetChildInfo("sphere");
            instance_rigid_body_technique_common_shape.cylinderChild = instance_rigid_body_technique_common_shape.Type.GetChildInfo("cylinder");
            instance_rigid_body_technique_common_shape.tapered_cylinderChild = instance_rigid_body_technique_common_shape.Type.GetChildInfo("tapered_cylinder");
            instance_rigid_body_technique_common_shape.capsuleChild = instance_rigid_body_technique_common_shape.Type.GetChildInfo("capsule");
            instance_rigid_body_technique_common_shape.tapered_capsuleChild = instance_rigid_body_technique_common_shape.Type.GetChildInfo("tapered_capsule");
            instance_rigid_body_technique_common_shape.translateChild = instance_rigid_body_technique_common_shape.Type.GetChildInfo("translate");
            instance_rigid_body_technique_common_shape.rotateChild = instance_rigid_body_technique_common_shape.Type.GetChildInfo("rotate");
            instance_rigid_body_technique_common_shape.extraChild = instance_rigid_body_technique_common_shape.Type.GetChildInfo("extra");

            technique_common_shape_hollow.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "technique_common_shape_hollow");
            technique_common_shape_hollow.Attribute = technique_common_shape_hollow.Type.GetAttributeInfo("");
            technique_common_shape_hollow.sidAttribute = technique_common_shape_hollow.Type.GetAttributeInfo("sid");

            instance_rigid_constraint.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "instance_rigid_constraint");
            instance_rigid_constraint.constraintAttribute = instance_rigid_constraint.Type.GetAttributeInfo("constraint");
            instance_rigid_constraint.sidAttribute = instance_rigid_constraint.Type.GetAttributeInfo("sid");
            instance_rigid_constraint.nameAttribute = instance_rigid_constraint.Type.GetAttributeInfo("name");
            instance_rigid_constraint.extraChild = instance_rigid_constraint.Type.GetChildInfo("extra");

            library_physics_scenes.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "library_physics_scenes");
            library_physics_scenes.idAttribute = library_physics_scenes.Type.GetAttributeInfo("id");
            library_physics_scenes.nameAttribute = library_physics_scenes.Type.GetAttributeInfo("name");
            library_physics_scenes.assetChild = library_physics_scenes.Type.GetChildInfo("asset");
            library_physics_scenes.physics_sceneChild = library_physics_scenes.Type.GetChildInfo("physics_scene");
            library_physics_scenes.extraChild = library_physics_scenes.Type.GetChildInfo("extra");

            physics_scene.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "physics_scene");
            physics_scene.idAttribute = physics_scene.Type.GetAttributeInfo("id");
            physics_scene.nameAttribute = physics_scene.Type.GetAttributeInfo("name");
            physics_scene.assetChild = physics_scene.Type.GetChildInfo("asset");
            physics_scene.instance_force_fieldChild = physics_scene.Type.GetChildInfo("instance_force_field");
            physics_scene.instance_physics_modelChild = physics_scene.Type.GetChildInfo("instance_physics_model");
            physics_scene.technique_commonChild = physics_scene.Type.GetChildInfo("technique_common");
            physics_scene.techniqueChild = physics_scene.Type.GetChildInfo("technique");
            physics_scene.extraChild = physics_scene.Type.GetChildInfo("extra");

            physics_scene_technique_common.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "physics_scene_technique_common");
            physics_scene_technique_common.gravityChild = physics_scene_technique_common.Type.GetChildInfo("gravity");
            physics_scene_technique_common.time_stepChild = physics_scene_technique_common.Type.GetChildInfo("time_step");

            library_visual_scenes.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "library_visual_scenes");
            library_visual_scenes.idAttribute = library_visual_scenes.Type.GetAttributeInfo("id");
            library_visual_scenes.nameAttribute = library_visual_scenes.Type.GetAttributeInfo("name");
            library_visual_scenes.assetChild = library_visual_scenes.Type.GetChildInfo("asset");
            library_visual_scenes.visual_sceneChild = library_visual_scenes.Type.GetChildInfo("visual_scene");
            library_visual_scenes.extraChild = library_visual_scenes.Type.GetChildInfo("extra");

            visual_scene.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "visual_scene");
            visual_scene.idAttribute = visual_scene.Type.GetAttributeInfo("id");
            visual_scene.nameAttribute = visual_scene.Type.GetAttributeInfo("name");
            visual_scene.assetChild = visual_scene.Type.GetChildInfo("asset");
            visual_scene.nodeChild = visual_scene.Type.GetChildInfo("node");
            visual_scene.evaluate_sceneChild = visual_scene.Type.GetChildInfo("evaluate_scene");
            visual_scene.extraChild = visual_scene.Type.GetChildInfo("extra");

            visual_scene_evaluate_scene.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "visual_scene_evaluate_scene");
            visual_scene_evaluate_scene.nameAttribute = visual_scene_evaluate_scene.Type.GetAttributeInfo("name");
            visual_scene_evaluate_scene.renderChild = visual_scene_evaluate_scene.Type.GetChildInfo("render");

            evaluate_scene_render.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "evaluate_scene_render");
            evaluate_scene_render.camera_nodeAttribute = evaluate_scene_render.Type.GetAttributeInfo("camera_node");
            evaluate_scene_render.layerChild = evaluate_scene_render.Type.GetChildInfo("layer");
            evaluate_scene_render.instance_effectChild = evaluate_scene_render.Type.GetChildInfo("instance_effect");

            COLLADA_scene.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "COLLADA_scene");
            COLLADA_scene.instance_physics_sceneChild = COLLADA_scene.Type.GetChildInfo("instance_physics_scene");
            COLLADA_scene.instance_visual_sceneChild = COLLADA_scene.Type.GetChildInfo("instance_visual_scene");
            COLLADA_scene.extraChild = COLLADA_scene.Type.GetChildInfo("extra");

            profile_GLSL.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "profile_GLSL");
            profile_GLSL.idAttribute = profile_GLSL.Type.GetAttributeInfo("id");
            profile_GLSL.assetChild = profile_GLSL.Type.GetChildInfo("asset");
            profile_GLSL.codeChild = profile_GLSL.Type.GetChildInfo("code");
            profile_GLSL.includeChild = profile_GLSL.Type.GetChildInfo("include");
            profile_GLSL.imageChild = profile_GLSL.Type.GetChildInfo("image");
            profile_GLSL.newparamChild = profile_GLSL.Type.GetChildInfo("newparam");
            profile_GLSL.techniqueChild = profile_GLSL.Type.GetChildInfo("technique");
            profile_GLSL.extraChild = profile_GLSL.Type.GetChildInfo("extra");

            glsl_newparam.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "glsl_newparam");
            glsl_newparam.semanticAttribute = glsl_newparam.Type.GetAttributeInfo("semantic");
            glsl_newparam.modifierAttribute = glsl_newparam.Type.GetAttributeInfo("modifier");
            glsl_newparam.boolAttribute = glsl_newparam.Type.GetAttributeInfo("bool");
            glsl_newparam.bool2Attribute = glsl_newparam.Type.GetAttributeInfo("bool2");
            glsl_newparam.bool3Attribute = glsl_newparam.Type.GetAttributeInfo("bool3");
            glsl_newparam.bool4Attribute = glsl_newparam.Type.GetAttributeInfo("bool4");
            glsl_newparam.floatAttribute = glsl_newparam.Type.GetAttributeInfo("float");
            glsl_newparam.float2Attribute = glsl_newparam.Type.GetAttributeInfo("float2");
            glsl_newparam.float3Attribute = glsl_newparam.Type.GetAttributeInfo("float3");
            glsl_newparam.float4Attribute = glsl_newparam.Type.GetAttributeInfo("float4");
            glsl_newparam.float2x2Attribute = glsl_newparam.Type.GetAttributeInfo("float2x2");
            glsl_newparam.float3x3Attribute = glsl_newparam.Type.GetAttributeInfo("float3x3");
            glsl_newparam.float4x4Attribute = glsl_newparam.Type.GetAttributeInfo("float4x4");
            glsl_newparam.intAttribute = glsl_newparam.Type.GetAttributeInfo("int");
            glsl_newparam.int2Attribute = glsl_newparam.Type.GetAttributeInfo("int2");
            glsl_newparam.int3Attribute = glsl_newparam.Type.GetAttributeInfo("int3");
            glsl_newparam.int4Attribute = glsl_newparam.Type.GetAttributeInfo("int4");
            glsl_newparam.enumAttribute = glsl_newparam.Type.GetAttributeInfo("enum");
            glsl_newparam.sidAttribute = glsl_newparam.Type.GetAttributeInfo("sid");
            glsl_newparam.annotateChild = glsl_newparam.Type.GetChildInfo("annotate");
            glsl_newparam.surfaceChild = glsl_newparam.Type.GetChildInfo("surface");
            glsl_newparam.sampler1DChild = glsl_newparam.Type.GetChildInfo("sampler1D");
            glsl_newparam.sampler2DChild = glsl_newparam.Type.GetChildInfo("sampler2D");
            glsl_newparam.sampler3DChild = glsl_newparam.Type.GetChildInfo("sampler3D");
            glsl_newparam.samplerCUBEChild = glsl_newparam.Type.GetChildInfo("samplerCUBE");
            glsl_newparam.samplerRECTChild = glsl_newparam.Type.GetChildInfo("samplerRECT");
            glsl_newparam.samplerDEPTHChild = glsl_newparam.Type.GetChildInfo("samplerDEPTH");
            glsl_newparam.arrayChild = glsl_newparam.Type.GetChildInfo("array");

            glsl_surface_type.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "glsl_surface_type");
            glsl_surface_type.formatAttribute = glsl_surface_type.Type.GetAttributeInfo("format");
            glsl_surface_type.sizeAttribute = glsl_surface_type.Type.GetAttributeInfo("size");
            glsl_surface_type.viewport_ratioAttribute = glsl_surface_type.Type.GetAttributeInfo("viewport_ratio");
            glsl_surface_type.mip_levelsAttribute = glsl_surface_type.Type.GetAttributeInfo("mip_levels");
            glsl_surface_type.mipmap_generateAttribute = glsl_surface_type.Type.GetAttributeInfo("mipmap_generate");
            glsl_surface_type.typeAttribute = glsl_surface_type.Type.GetAttributeInfo("type");
            glsl_surface_type.init_as_nullChild = glsl_surface_type.Type.GetChildInfo("init_as_null");
            glsl_surface_type.init_as_targetChild = glsl_surface_type.Type.GetChildInfo("init_as_target");
            glsl_surface_type.init_cubeChild = glsl_surface_type.Type.GetChildInfo("init_cube");
            glsl_surface_type.init_volumeChild = glsl_surface_type.Type.GetChildInfo("init_volume");
            glsl_surface_type.init_planarChild = glsl_surface_type.Type.GetChildInfo("init_planar");
            glsl_surface_type.init_fromChild = glsl_surface_type.Type.GetChildInfo("init_from");
            glsl_surface_type.format_hintChild = glsl_surface_type.Type.GetChildInfo("format_hint");
            glsl_surface_type.extraChild = glsl_surface_type.Type.GetChildInfo("extra");
            glsl_surface_type.generatorChild = glsl_surface_type.Type.GetChildInfo("generator");

            glsl_surface_type_generator.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "glsl_surface_type_generator");
            glsl_surface_type_generator.annotateChild = glsl_surface_type_generator.Type.GetChildInfo("annotate");
            glsl_surface_type_generator.codeChild = glsl_surface_type_generator.Type.GetChildInfo("code");
            glsl_surface_type_generator.includeChild = glsl_surface_type_generator.Type.GetChildInfo("include");
            glsl_surface_type_generator.nameChild = glsl_surface_type_generator.Type.GetChildInfo("name");
            glsl_surface_type_generator.setparamChild = glsl_surface_type_generator.Type.GetChildInfo("setparam");

            glsl_surface_type_generator_name.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "glsl_surface_type_generator_name");
            glsl_surface_type_generator_name.Attribute = glsl_surface_type_generator_name.Type.GetAttributeInfo("");
            glsl_surface_type_generator_name.sourceAttribute = glsl_surface_type_generator_name.Type.GetAttributeInfo("source");

            glsl_setparam_simple.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "glsl_setparam_simple");
            glsl_setparam_simple.boolAttribute = glsl_setparam_simple.Type.GetAttributeInfo("bool");
            glsl_setparam_simple.bool2Attribute = glsl_setparam_simple.Type.GetAttributeInfo("bool2");
            glsl_setparam_simple.bool3Attribute = glsl_setparam_simple.Type.GetAttributeInfo("bool3");
            glsl_setparam_simple.bool4Attribute = glsl_setparam_simple.Type.GetAttributeInfo("bool4");
            glsl_setparam_simple.floatAttribute = glsl_setparam_simple.Type.GetAttributeInfo("float");
            glsl_setparam_simple.float2Attribute = glsl_setparam_simple.Type.GetAttributeInfo("float2");
            glsl_setparam_simple.float3Attribute = glsl_setparam_simple.Type.GetAttributeInfo("float3");
            glsl_setparam_simple.float4Attribute = glsl_setparam_simple.Type.GetAttributeInfo("float4");
            glsl_setparam_simple.float2x2Attribute = glsl_setparam_simple.Type.GetAttributeInfo("float2x2");
            glsl_setparam_simple.float3x3Attribute = glsl_setparam_simple.Type.GetAttributeInfo("float3x3");
            glsl_setparam_simple.float4x4Attribute = glsl_setparam_simple.Type.GetAttributeInfo("float4x4");
            glsl_setparam_simple.intAttribute = glsl_setparam_simple.Type.GetAttributeInfo("int");
            glsl_setparam_simple.int2Attribute = glsl_setparam_simple.Type.GetAttributeInfo("int2");
            glsl_setparam_simple.int3Attribute = glsl_setparam_simple.Type.GetAttributeInfo("int3");
            glsl_setparam_simple.int4Attribute = glsl_setparam_simple.Type.GetAttributeInfo("int4");
            glsl_setparam_simple.enumAttribute = glsl_setparam_simple.Type.GetAttributeInfo("enum");
            glsl_setparam_simple.refAttribute = glsl_setparam_simple.Type.GetAttributeInfo("ref");
            glsl_setparam_simple.annotateChild = glsl_setparam_simple.Type.GetChildInfo("annotate");
            glsl_setparam_simple.surfaceChild = glsl_setparam_simple.Type.GetChildInfo("surface");
            glsl_setparam_simple.sampler1DChild = glsl_setparam_simple.Type.GetChildInfo("sampler1D");
            glsl_setparam_simple.sampler2DChild = glsl_setparam_simple.Type.GetChildInfo("sampler2D");
            glsl_setparam_simple.sampler3DChild = glsl_setparam_simple.Type.GetChildInfo("sampler3D");
            glsl_setparam_simple.samplerCUBEChild = glsl_setparam_simple.Type.GetChildInfo("samplerCUBE");
            glsl_setparam_simple.samplerRECTChild = glsl_setparam_simple.Type.GetChildInfo("samplerRECT");
            glsl_setparam_simple.samplerDEPTHChild = glsl_setparam_simple.Type.GetChildInfo("samplerDEPTH");

            glsl_newarray_type.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "glsl_newarray_type");
            glsl_newarray_type.boolAttribute = glsl_newarray_type.Type.GetAttributeInfo("bool");
            glsl_newarray_type.bool2Attribute = glsl_newarray_type.Type.GetAttributeInfo("bool2");
            glsl_newarray_type.bool3Attribute = glsl_newarray_type.Type.GetAttributeInfo("bool3");
            glsl_newarray_type.bool4Attribute = glsl_newarray_type.Type.GetAttributeInfo("bool4");
            glsl_newarray_type.floatAttribute = glsl_newarray_type.Type.GetAttributeInfo("float");
            glsl_newarray_type.float2Attribute = glsl_newarray_type.Type.GetAttributeInfo("float2");
            glsl_newarray_type.float3Attribute = glsl_newarray_type.Type.GetAttributeInfo("float3");
            glsl_newarray_type.float4Attribute = glsl_newarray_type.Type.GetAttributeInfo("float4");
            glsl_newarray_type.float2x2Attribute = glsl_newarray_type.Type.GetAttributeInfo("float2x2");
            glsl_newarray_type.float3x3Attribute = glsl_newarray_type.Type.GetAttributeInfo("float3x3");
            glsl_newarray_type.float4x4Attribute = glsl_newarray_type.Type.GetAttributeInfo("float4x4");
            glsl_newarray_type.intAttribute = glsl_newarray_type.Type.GetAttributeInfo("int");
            glsl_newarray_type.int2Attribute = glsl_newarray_type.Type.GetAttributeInfo("int2");
            glsl_newarray_type.int3Attribute = glsl_newarray_type.Type.GetAttributeInfo("int3");
            glsl_newarray_type.int4Attribute = glsl_newarray_type.Type.GetAttributeInfo("int4");
            glsl_newarray_type.enumAttribute = glsl_newarray_type.Type.GetAttributeInfo("enum");
            glsl_newarray_type.lengthAttribute = glsl_newarray_type.Type.GetAttributeInfo("length");
            glsl_newarray_type.surfaceChild = glsl_newarray_type.Type.GetChildInfo("surface");
            glsl_newarray_type.sampler1DChild = glsl_newarray_type.Type.GetChildInfo("sampler1D");
            glsl_newarray_type.sampler2DChild = glsl_newarray_type.Type.GetChildInfo("sampler2D");
            glsl_newarray_type.sampler3DChild = glsl_newarray_type.Type.GetChildInfo("sampler3D");
            glsl_newarray_type.samplerCUBEChild = glsl_newarray_type.Type.GetChildInfo("samplerCUBE");
            glsl_newarray_type.samplerRECTChild = glsl_newarray_type.Type.GetChildInfo("samplerRECT");
            glsl_newarray_type.samplerDEPTHChild = glsl_newarray_type.Type.GetChildInfo("samplerDEPTH");
            glsl_newarray_type.arrayChild = glsl_newarray_type.Type.GetChildInfo("array");

            profile_GLSL_technique.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "profile_GLSL_technique");
            profile_GLSL_technique.idAttribute = profile_GLSL_technique.Type.GetAttributeInfo("id");
            profile_GLSL_technique.sidAttribute = profile_GLSL_technique.Type.GetAttributeInfo("sid");
            profile_GLSL_technique.annotateChild = profile_GLSL_technique.Type.GetChildInfo("annotate");
            profile_GLSL_technique.codeChild = profile_GLSL_technique.Type.GetChildInfo("code");
            profile_GLSL_technique.includeChild = profile_GLSL_technique.Type.GetChildInfo("include");
            profile_GLSL_technique.imageChild = profile_GLSL_technique.Type.GetChildInfo("image");
            profile_GLSL_technique.newparamChild = profile_GLSL_technique.Type.GetChildInfo("newparam");
            profile_GLSL_technique.setparamChild = profile_GLSL_technique.Type.GetChildInfo("setparam");
            profile_GLSL_technique.passChild = profile_GLSL_technique.Type.GetChildInfo("pass");
            profile_GLSL_technique.extraChild = profile_GLSL_technique.Type.GetChildInfo("extra");

            glsl_setparam.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "glsl_setparam");
            glsl_setparam.boolAttribute = glsl_setparam.Type.GetAttributeInfo("bool");
            glsl_setparam.bool2Attribute = glsl_setparam.Type.GetAttributeInfo("bool2");
            glsl_setparam.bool3Attribute = glsl_setparam.Type.GetAttributeInfo("bool3");
            glsl_setparam.bool4Attribute = glsl_setparam.Type.GetAttributeInfo("bool4");
            glsl_setparam.floatAttribute = glsl_setparam.Type.GetAttributeInfo("float");
            glsl_setparam.float2Attribute = glsl_setparam.Type.GetAttributeInfo("float2");
            glsl_setparam.float3Attribute = glsl_setparam.Type.GetAttributeInfo("float3");
            glsl_setparam.float4Attribute = glsl_setparam.Type.GetAttributeInfo("float4");
            glsl_setparam.float2x2Attribute = glsl_setparam.Type.GetAttributeInfo("float2x2");
            glsl_setparam.float3x3Attribute = glsl_setparam.Type.GetAttributeInfo("float3x3");
            glsl_setparam.float4x4Attribute = glsl_setparam.Type.GetAttributeInfo("float4x4");
            glsl_setparam.intAttribute = glsl_setparam.Type.GetAttributeInfo("int");
            glsl_setparam.int2Attribute = glsl_setparam.Type.GetAttributeInfo("int2");
            glsl_setparam.int3Attribute = glsl_setparam.Type.GetAttributeInfo("int3");
            glsl_setparam.int4Attribute = glsl_setparam.Type.GetAttributeInfo("int4");
            glsl_setparam.enumAttribute = glsl_setparam.Type.GetAttributeInfo("enum");
            glsl_setparam.refAttribute = glsl_setparam.Type.GetAttributeInfo("ref");
            glsl_setparam.programAttribute = glsl_setparam.Type.GetAttributeInfo("program");
            glsl_setparam.annotateChild = glsl_setparam.Type.GetChildInfo("annotate");
            glsl_setparam.surfaceChild = glsl_setparam.Type.GetChildInfo("surface");
            glsl_setparam.sampler1DChild = glsl_setparam.Type.GetChildInfo("sampler1D");
            glsl_setparam.sampler2DChild = glsl_setparam.Type.GetChildInfo("sampler2D");
            glsl_setparam.sampler3DChild = glsl_setparam.Type.GetChildInfo("sampler3D");
            glsl_setparam.samplerCUBEChild = glsl_setparam.Type.GetChildInfo("samplerCUBE");
            glsl_setparam.samplerRECTChild = glsl_setparam.Type.GetChildInfo("samplerRECT");
            glsl_setparam.samplerDEPTHChild = glsl_setparam.Type.GetChildInfo("samplerDEPTH");
            glsl_setparam.arrayChild = glsl_setparam.Type.GetChildInfo("array");

            glsl_setarray_type.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "glsl_setarray_type");
            glsl_setarray_type.boolAttribute = glsl_setarray_type.Type.GetAttributeInfo("bool");
            glsl_setarray_type.bool2Attribute = glsl_setarray_type.Type.GetAttributeInfo("bool2");
            glsl_setarray_type.bool3Attribute = glsl_setarray_type.Type.GetAttributeInfo("bool3");
            glsl_setarray_type.bool4Attribute = glsl_setarray_type.Type.GetAttributeInfo("bool4");
            glsl_setarray_type.floatAttribute = glsl_setarray_type.Type.GetAttributeInfo("float");
            glsl_setarray_type.float2Attribute = glsl_setarray_type.Type.GetAttributeInfo("float2");
            glsl_setarray_type.float3Attribute = glsl_setarray_type.Type.GetAttributeInfo("float3");
            glsl_setarray_type.float4Attribute = glsl_setarray_type.Type.GetAttributeInfo("float4");
            glsl_setarray_type.float2x2Attribute = glsl_setarray_type.Type.GetAttributeInfo("float2x2");
            glsl_setarray_type.float3x3Attribute = glsl_setarray_type.Type.GetAttributeInfo("float3x3");
            glsl_setarray_type.float4x4Attribute = glsl_setarray_type.Type.GetAttributeInfo("float4x4");
            glsl_setarray_type.intAttribute = glsl_setarray_type.Type.GetAttributeInfo("int");
            glsl_setarray_type.int2Attribute = glsl_setarray_type.Type.GetAttributeInfo("int2");
            glsl_setarray_type.int3Attribute = glsl_setarray_type.Type.GetAttributeInfo("int3");
            glsl_setarray_type.int4Attribute = glsl_setarray_type.Type.GetAttributeInfo("int4");
            glsl_setarray_type.enumAttribute = glsl_setarray_type.Type.GetAttributeInfo("enum");
            glsl_setarray_type.lengthAttribute = glsl_setarray_type.Type.GetAttributeInfo("length");
            glsl_setarray_type.surfaceChild = glsl_setarray_type.Type.GetChildInfo("surface");
            glsl_setarray_type.sampler1DChild = glsl_setarray_type.Type.GetChildInfo("sampler1D");
            glsl_setarray_type.sampler2DChild = glsl_setarray_type.Type.GetChildInfo("sampler2D");
            glsl_setarray_type.sampler3DChild = glsl_setarray_type.Type.GetChildInfo("sampler3D");
            glsl_setarray_type.samplerCUBEChild = glsl_setarray_type.Type.GetChildInfo("samplerCUBE");
            glsl_setarray_type.samplerRECTChild = glsl_setarray_type.Type.GetChildInfo("samplerRECT");
            glsl_setarray_type.samplerDEPTHChild = glsl_setarray_type.Type.GetChildInfo("samplerDEPTH");
            glsl_setarray_type.arrayChild = glsl_setarray_type.Type.GetChildInfo("array");

            profile_GLSL_technique_pass.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "profile_GLSL_technique_pass");
            profile_GLSL_technique_pass.drawAttribute = profile_GLSL_technique_pass.Type.GetAttributeInfo("draw");
            profile_GLSL_technique_pass.sidAttribute = profile_GLSL_technique_pass.Type.GetAttributeInfo("sid");
            profile_GLSL_technique_pass.annotateChild = profile_GLSL_technique_pass.Type.GetChildInfo("annotate");
            profile_GLSL_technique_pass.color_targetChild = profile_GLSL_technique_pass.Type.GetChildInfo("color_target");
            profile_GLSL_technique_pass.depth_targetChild = profile_GLSL_technique_pass.Type.GetChildInfo("depth_target");
            profile_GLSL_technique_pass.stencil_targetChild = profile_GLSL_technique_pass.Type.GetChildInfo("stencil_target");
            profile_GLSL_technique_pass.color_clearChild = profile_GLSL_technique_pass.Type.GetChildInfo("color_clear");
            profile_GLSL_technique_pass.depth_clearChild = profile_GLSL_technique_pass.Type.GetChildInfo("depth_clear");
            profile_GLSL_technique_pass.stencil_clearChild = profile_GLSL_technique_pass.Type.GetChildInfo("stencil_clear");
            profile_GLSL_technique_pass.alpha_funcChild = profile_GLSL_technique_pass.Type.GetChildInfo("alpha_func");
            profile_GLSL_technique_pass.blend_funcChild = profile_GLSL_technique_pass.Type.GetChildInfo("blend_func");
            profile_GLSL_technique_pass.blend_func_separateChild = profile_GLSL_technique_pass.Type.GetChildInfo("blend_func_separate");
            profile_GLSL_technique_pass.blend_equationChild = profile_GLSL_technique_pass.Type.GetChildInfo("blend_equation");
            profile_GLSL_technique_pass.blend_equation_separateChild = profile_GLSL_technique_pass.Type.GetChildInfo("blend_equation_separate");
            profile_GLSL_technique_pass.color_materialChild = profile_GLSL_technique_pass.Type.GetChildInfo("color_material");
            profile_GLSL_technique_pass.cull_faceChild = profile_GLSL_technique_pass.Type.GetChildInfo("cull_face");
            profile_GLSL_technique_pass.depth_funcChild = profile_GLSL_technique_pass.Type.GetChildInfo("depth_func");
            profile_GLSL_technique_pass.fog_modeChild = profile_GLSL_technique_pass.Type.GetChildInfo("fog_mode");
            profile_GLSL_technique_pass.fog_coord_srcChild = profile_GLSL_technique_pass.Type.GetChildInfo("fog_coord_src");
            profile_GLSL_technique_pass.front_faceChild = profile_GLSL_technique_pass.Type.GetChildInfo("front_face");
            profile_GLSL_technique_pass.light_model_color_controlChild = profile_GLSL_technique_pass.Type.GetChildInfo("light_model_color_control");
            profile_GLSL_technique_pass.logic_opChild = profile_GLSL_technique_pass.Type.GetChildInfo("logic_op");
            profile_GLSL_technique_pass.polygon_modeChild = profile_GLSL_technique_pass.Type.GetChildInfo("polygon_mode");
            profile_GLSL_technique_pass.shade_modelChild = profile_GLSL_technique_pass.Type.GetChildInfo("shade_model");
            profile_GLSL_technique_pass.stencil_funcChild = profile_GLSL_technique_pass.Type.GetChildInfo("stencil_func");
            profile_GLSL_technique_pass.stencil_opChild = profile_GLSL_technique_pass.Type.GetChildInfo("stencil_op");
            profile_GLSL_technique_pass.stencil_func_separateChild = profile_GLSL_technique_pass.Type.GetChildInfo("stencil_func_separate");
            profile_GLSL_technique_pass.stencil_op_separateChild = profile_GLSL_technique_pass.Type.GetChildInfo("stencil_op_separate");
            profile_GLSL_technique_pass.stencil_mask_separateChild = profile_GLSL_technique_pass.Type.GetChildInfo("stencil_mask_separate");
            profile_GLSL_technique_pass.light_enableChild = profile_GLSL_technique_pass.Type.GetChildInfo("light_enable");
            profile_GLSL_technique_pass.light_ambientChild = profile_GLSL_technique_pass.Type.GetChildInfo("light_ambient");
            profile_GLSL_technique_pass.light_diffuseChild = profile_GLSL_technique_pass.Type.GetChildInfo("light_diffuse");
            profile_GLSL_technique_pass.light_specularChild = profile_GLSL_technique_pass.Type.GetChildInfo("light_specular");
            profile_GLSL_technique_pass.light_positionChild = profile_GLSL_technique_pass.Type.GetChildInfo("light_position");
            profile_GLSL_technique_pass.light_constant_attenuationChild = profile_GLSL_technique_pass.Type.GetChildInfo("light_constant_attenuation");
            profile_GLSL_technique_pass.light_linear_attenuationChild = profile_GLSL_technique_pass.Type.GetChildInfo("light_linear_attenuation");
            profile_GLSL_technique_pass.light_quadratic_attenuationChild = profile_GLSL_technique_pass.Type.GetChildInfo("light_quadratic_attenuation");
            profile_GLSL_technique_pass.light_spot_cutoffChild = profile_GLSL_technique_pass.Type.GetChildInfo("light_spot_cutoff");
            profile_GLSL_technique_pass.light_spot_directionChild = profile_GLSL_technique_pass.Type.GetChildInfo("light_spot_direction");
            profile_GLSL_technique_pass.light_spot_exponentChild = profile_GLSL_technique_pass.Type.GetChildInfo("light_spot_exponent");
            profile_GLSL_technique_pass.texture1DChild = profile_GLSL_technique_pass.Type.GetChildInfo("texture1D");
            profile_GLSL_technique_pass.texture2DChild = profile_GLSL_technique_pass.Type.GetChildInfo("texture2D");
            profile_GLSL_technique_pass.texture3DChild = profile_GLSL_technique_pass.Type.GetChildInfo("texture3D");
            profile_GLSL_technique_pass.textureCUBEChild = profile_GLSL_technique_pass.Type.GetChildInfo("textureCUBE");
            profile_GLSL_technique_pass.textureRECTChild = profile_GLSL_technique_pass.Type.GetChildInfo("textureRECT");
            profile_GLSL_technique_pass.textureDEPTHChild = profile_GLSL_technique_pass.Type.GetChildInfo("textureDEPTH");
            profile_GLSL_technique_pass.texture1D_enableChild = profile_GLSL_technique_pass.Type.GetChildInfo("texture1D_enable");
            profile_GLSL_technique_pass.texture2D_enableChild = profile_GLSL_technique_pass.Type.GetChildInfo("texture2D_enable");
            profile_GLSL_technique_pass.texture3D_enableChild = profile_GLSL_technique_pass.Type.GetChildInfo("texture3D_enable");
            profile_GLSL_technique_pass.textureCUBE_enableChild = profile_GLSL_technique_pass.Type.GetChildInfo("textureCUBE_enable");
            profile_GLSL_technique_pass.textureRECT_enableChild = profile_GLSL_technique_pass.Type.GetChildInfo("textureRECT_enable");
            profile_GLSL_technique_pass.textureDEPTH_enableChild = profile_GLSL_technique_pass.Type.GetChildInfo("textureDEPTH_enable");
            profile_GLSL_technique_pass.texture_env_colorChild = profile_GLSL_technique_pass.Type.GetChildInfo("texture_env_color");
            profile_GLSL_technique_pass.texture_env_modeChild = profile_GLSL_technique_pass.Type.GetChildInfo("texture_env_mode");
            profile_GLSL_technique_pass.clip_planeChild = profile_GLSL_technique_pass.Type.GetChildInfo("clip_plane");
            profile_GLSL_technique_pass.clip_plane_enableChild = profile_GLSL_technique_pass.Type.GetChildInfo("clip_plane_enable");
            profile_GLSL_technique_pass.blend_colorChild = profile_GLSL_technique_pass.Type.GetChildInfo("blend_color");
            profile_GLSL_technique_pass.clear_colorChild = profile_GLSL_technique_pass.Type.GetChildInfo("clear_color");
            profile_GLSL_technique_pass.clear_stencilChild = profile_GLSL_technique_pass.Type.GetChildInfo("clear_stencil");
            profile_GLSL_technique_pass.clear_depthChild = profile_GLSL_technique_pass.Type.GetChildInfo("clear_depth");
            profile_GLSL_technique_pass.color_maskChild = profile_GLSL_technique_pass.Type.GetChildInfo("color_mask");
            profile_GLSL_technique_pass.depth_boundsChild = profile_GLSL_technique_pass.Type.GetChildInfo("depth_bounds");
            profile_GLSL_technique_pass.depth_maskChild = profile_GLSL_technique_pass.Type.GetChildInfo("depth_mask");
            profile_GLSL_technique_pass.depth_rangeChild = profile_GLSL_technique_pass.Type.GetChildInfo("depth_range");
            profile_GLSL_technique_pass.fog_densityChild = profile_GLSL_technique_pass.Type.GetChildInfo("fog_density");
            profile_GLSL_technique_pass.fog_startChild = profile_GLSL_technique_pass.Type.GetChildInfo("fog_start");
            profile_GLSL_technique_pass.fog_endChild = profile_GLSL_technique_pass.Type.GetChildInfo("fog_end");
            profile_GLSL_technique_pass.fog_colorChild = profile_GLSL_technique_pass.Type.GetChildInfo("fog_color");
            profile_GLSL_technique_pass.light_model_ambientChild = profile_GLSL_technique_pass.Type.GetChildInfo("light_model_ambient");
            profile_GLSL_technique_pass.lighting_enableChild = profile_GLSL_technique_pass.Type.GetChildInfo("lighting_enable");
            profile_GLSL_technique_pass.line_stippleChild = profile_GLSL_technique_pass.Type.GetChildInfo("line_stipple");
            profile_GLSL_technique_pass.line_widthChild = profile_GLSL_technique_pass.Type.GetChildInfo("line_width");
            profile_GLSL_technique_pass.material_ambientChild = profile_GLSL_technique_pass.Type.GetChildInfo("material_ambient");
            profile_GLSL_technique_pass.material_diffuseChild = profile_GLSL_technique_pass.Type.GetChildInfo("material_diffuse");
            profile_GLSL_technique_pass.material_emissionChild = profile_GLSL_technique_pass.Type.GetChildInfo("material_emission");
            profile_GLSL_technique_pass.material_shininessChild = profile_GLSL_technique_pass.Type.GetChildInfo("material_shininess");
            profile_GLSL_technique_pass.material_specularChild = profile_GLSL_technique_pass.Type.GetChildInfo("material_specular");
            profile_GLSL_technique_pass.model_view_matrixChild = profile_GLSL_technique_pass.Type.GetChildInfo("model_view_matrix");
            profile_GLSL_technique_pass.point_distance_attenuationChild = profile_GLSL_technique_pass.Type.GetChildInfo("point_distance_attenuation");
            profile_GLSL_technique_pass.point_fade_threshold_sizeChild = profile_GLSL_technique_pass.Type.GetChildInfo("point_fade_threshold_size");
            profile_GLSL_technique_pass.point_sizeChild = profile_GLSL_technique_pass.Type.GetChildInfo("point_size");
            profile_GLSL_technique_pass.point_size_minChild = profile_GLSL_technique_pass.Type.GetChildInfo("point_size_min");
            profile_GLSL_technique_pass.point_size_maxChild = profile_GLSL_technique_pass.Type.GetChildInfo("point_size_max");
            profile_GLSL_technique_pass.polygon_offsetChild = profile_GLSL_technique_pass.Type.GetChildInfo("polygon_offset");
            profile_GLSL_technique_pass.projection_matrixChild = profile_GLSL_technique_pass.Type.GetChildInfo("projection_matrix");
            profile_GLSL_technique_pass.scissorChild = profile_GLSL_technique_pass.Type.GetChildInfo("scissor");
            profile_GLSL_technique_pass.stencil_maskChild = profile_GLSL_technique_pass.Type.GetChildInfo("stencil_mask");
            profile_GLSL_technique_pass.alpha_test_enableChild = profile_GLSL_technique_pass.Type.GetChildInfo("alpha_test_enable");
            profile_GLSL_technique_pass.auto_normal_enableChild = profile_GLSL_technique_pass.Type.GetChildInfo("auto_normal_enable");
            profile_GLSL_technique_pass.blend_enableChild = profile_GLSL_technique_pass.Type.GetChildInfo("blend_enable");
            profile_GLSL_technique_pass.color_logic_op_enableChild = profile_GLSL_technique_pass.Type.GetChildInfo("color_logic_op_enable");
            profile_GLSL_technique_pass.color_material_enableChild = profile_GLSL_technique_pass.Type.GetChildInfo("color_material_enable");
            profile_GLSL_technique_pass.cull_face_enableChild = profile_GLSL_technique_pass.Type.GetChildInfo("cull_face_enable");
            profile_GLSL_technique_pass.depth_bounds_enableChild = profile_GLSL_technique_pass.Type.GetChildInfo("depth_bounds_enable");
            profile_GLSL_technique_pass.depth_clamp_enableChild = profile_GLSL_technique_pass.Type.GetChildInfo("depth_clamp_enable");
            profile_GLSL_technique_pass.depth_test_enableChild = profile_GLSL_technique_pass.Type.GetChildInfo("depth_test_enable");
            profile_GLSL_technique_pass.dither_enableChild = profile_GLSL_technique_pass.Type.GetChildInfo("dither_enable");
            profile_GLSL_technique_pass.fog_enableChild = profile_GLSL_technique_pass.Type.GetChildInfo("fog_enable");
            profile_GLSL_technique_pass.light_model_local_viewer_enableChild = profile_GLSL_technique_pass.Type.GetChildInfo("light_model_local_viewer_enable");
            profile_GLSL_technique_pass.light_model_two_side_enableChild = profile_GLSL_technique_pass.Type.GetChildInfo("light_model_two_side_enable");
            profile_GLSL_technique_pass.line_smooth_enableChild = profile_GLSL_technique_pass.Type.GetChildInfo("line_smooth_enable");
            profile_GLSL_technique_pass.line_stipple_enableChild = profile_GLSL_technique_pass.Type.GetChildInfo("line_stipple_enable");
            profile_GLSL_technique_pass.logic_op_enableChild = profile_GLSL_technique_pass.Type.GetChildInfo("logic_op_enable");
            profile_GLSL_technique_pass.multisample_enableChild = profile_GLSL_technique_pass.Type.GetChildInfo("multisample_enable");
            profile_GLSL_technique_pass.normalize_enableChild = profile_GLSL_technique_pass.Type.GetChildInfo("normalize_enable");
            profile_GLSL_technique_pass.point_smooth_enableChild = profile_GLSL_technique_pass.Type.GetChildInfo("point_smooth_enable");
            profile_GLSL_technique_pass.polygon_offset_fill_enableChild = profile_GLSL_technique_pass.Type.GetChildInfo("polygon_offset_fill_enable");
            profile_GLSL_technique_pass.polygon_offset_line_enableChild = profile_GLSL_technique_pass.Type.GetChildInfo("polygon_offset_line_enable");
            profile_GLSL_technique_pass.polygon_offset_point_enableChild = profile_GLSL_technique_pass.Type.GetChildInfo("polygon_offset_point_enable");
            profile_GLSL_technique_pass.polygon_smooth_enableChild = profile_GLSL_technique_pass.Type.GetChildInfo("polygon_smooth_enable");
            profile_GLSL_technique_pass.polygon_stipple_enableChild = profile_GLSL_technique_pass.Type.GetChildInfo("polygon_stipple_enable");
            profile_GLSL_technique_pass.rescale_normal_enableChild = profile_GLSL_technique_pass.Type.GetChildInfo("rescale_normal_enable");
            profile_GLSL_technique_pass.sample_alpha_to_coverage_enableChild = profile_GLSL_technique_pass.Type.GetChildInfo("sample_alpha_to_coverage_enable");
            profile_GLSL_technique_pass.sample_alpha_to_one_enableChild = profile_GLSL_technique_pass.Type.GetChildInfo("sample_alpha_to_one_enable");
            profile_GLSL_technique_pass.sample_coverage_enableChild = profile_GLSL_technique_pass.Type.GetChildInfo("sample_coverage_enable");
            profile_GLSL_technique_pass.scissor_test_enableChild = profile_GLSL_technique_pass.Type.GetChildInfo("scissor_test_enable");
            profile_GLSL_technique_pass.stencil_test_enableChild = profile_GLSL_technique_pass.Type.GetChildInfo("stencil_test_enable");
            profile_GLSL_technique_pass.gl_hook_abstractChild = profile_GLSL_technique_pass.Type.GetChildInfo("gl_hook_abstract");
            profile_GLSL_technique_pass.shaderChild = profile_GLSL_technique_pass.Type.GetChildInfo("shader");
            profile_GLSL_technique_pass.extraChild = profile_GLSL_technique_pass.Type.GetChildInfo("extra");

            technique_pass_shader.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "technique_pass_shader");
            technique_pass_shader.compiler_optionsAttribute = technique_pass_shader.Type.GetAttributeInfo("compiler_options");
            technique_pass_shader.stageAttribute = technique_pass_shader.Type.GetAttributeInfo("stage");
            technique_pass_shader.annotateChild = technique_pass_shader.Type.GetChildInfo("annotate");
            technique_pass_shader.compiler_targetChild = technique_pass_shader.Type.GetChildInfo("compiler_target");
            technique_pass_shader.nameChild = technique_pass_shader.Type.GetChildInfo("name");
            technique_pass_shader.bindChild = technique_pass_shader.Type.GetChildInfo("bind");

            pass_shader_compiler_target.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "pass_shader_compiler_target");
            pass_shader_compiler_target.Attribute = pass_shader_compiler_target.Type.GetAttributeInfo("");

            pass_shader_name.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "pass_shader_name");
            pass_shader_name.Attribute = pass_shader_name.Type.GetAttributeInfo("");
            pass_shader_name.sourceAttribute = pass_shader_name.Type.GetAttributeInfo("source");

            pass_shader_bind.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "pass_shader_bind");
            pass_shader_bind.boolAttribute = pass_shader_bind.Type.GetAttributeInfo("bool");
            pass_shader_bind.bool2Attribute = pass_shader_bind.Type.GetAttributeInfo("bool2");
            pass_shader_bind.bool3Attribute = pass_shader_bind.Type.GetAttributeInfo("bool3");
            pass_shader_bind.bool4Attribute = pass_shader_bind.Type.GetAttributeInfo("bool4");
            pass_shader_bind.floatAttribute = pass_shader_bind.Type.GetAttributeInfo("float");
            pass_shader_bind.float2Attribute = pass_shader_bind.Type.GetAttributeInfo("float2");
            pass_shader_bind.float3Attribute = pass_shader_bind.Type.GetAttributeInfo("float3");
            pass_shader_bind.float4Attribute = pass_shader_bind.Type.GetAttributeInfo("float4");
            pass_shader_bind.float2x2Attribute = pass_shader_bind.Type.GetAttributeInfo("float2x2");
            pass_shader_bind.float3x3Attribute = pass_shader_bind.Type.GetAttributeInfo("float3x3");
            pass_shader_bind.float4x4Attribute = pass_shader_bind.Type.GetAttributeInfo("float4x4");
            pass_shader_bind.intAttribute = pass_shader_bind.Type.GetAttributeInfo("int");
            pass_shader_bind.int2Attribute = pass_shader_bind.Type.GetAttributeInfo("int2");
            pass_shader_bind.int3Attribute = pass_shader_bind.Type.GetAttributeInfo("int3");
            pass_shader_bind.int4Attribute = pass_shader_bind.Type.GetAttributeInfo("int4");
            pass_shader_bind.enumAttribute = pass_shader_bind.Type.GetAttributeInfo("enum");
            pass_shader_bind.symbolAttribute = pass_shader_bind.Type.GetAttributeInfo("symbol");
            pass_shader_bind.surfaceChild = pass_shader_bind.Type.GetChildInfo("surface");
            pass_shader_bind.sampler1DChild = pass_shader_bind.Type.GetChildInfo("sampler1D");
            pass_shader_bind.sampler2DChild = pass_shader_bind.Type.GetChildInfo("sampler2D");
            pass_shader_bind.sampler3DChild = pass_shader_bind.Type.GetChildInfo("sampler3D");
            pass_shader_bind.samplerCUBEChild = pass_shader_bind.Type.GetChildInfo("samplerCUBE");
            pass_shader_bind.samplerRECTChild = pass_shader_bind.Type.GetChildInfo("samplerRECT");
            pass_shader_bind.samplerDEPTHChild = pass_shader_bind.Type.GetChildInfo("samplerDEPTH");
            pass_shader_bind.paramChild = pass_shader_bind.Type.GetChildInfo("param");

            shader_bind_param.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "shader_bind_param");
            shader_bind_param.refAttribute = shader_bind_param.Type.GetAttributeInfo("ref");

            profile_GLES.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "profile_GLES");
            profile_GLES.idAttribute = profile_GLES.Type.GetAttributeInfo("id");
            profile_GLES.platformAttribute = profile_GLES.Type.GetAttributeInfo("platform");
            profile_GLES.assetChild = profile_GLES.Type.GetChildInfo("asset");
            profile_GLES.imageChild = profile_GLES.Type.GetChildInfo("image");
            profile_GLES.newparamChild = profile_GLES.Type.GetChildInfo("newparam");
            profile_GLES.techniqueChild = profile_GLES.Type.GetChildInfo("technique");
            profile_GLES.extraChild = profile_GLES.Type.GetChildInfo("extra");

            gles_newparam.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "gles_newparam");
            gles_newparam.semanticAttribute = gles_newparam.Type.GetAttributeInfo("semantic");
            gles_newparam.modifierAttribute = gles_newparam.Type.GetAttributeInfo("modifier");
            gles_newparam.boolAttribute = gles_newparam.Type.GetAttributeInfo("bool");
            gles_newparam.bool2Attribute = gles_newparam.Type.GetAttributeInfo("bool2");
            gles_newparam.bool3Attribute = gles_newparam.Type.GetAttributeInfo("bool3");
            gles_newparam.bool4Attribute = gles_newparam.Type.GetAttributeInfo("bool4");
            gles_newparam.intAttribute = gles_newparam.Type.GetAttributeInfo("int");
            gles_newparam.int2Attribute = gles_newparam.Type.GetAttributeInfo("int2");
            gles_newparam.int3Attribute = gles_newparam.Type.GetAttributeInfo("int3");
            gles_newparam.int4Attribute = gles_newparam.Type.GetAttributeInfo("int4");
            gles_newparam.floatAttribute = gles_newparam.Type.GetAttributeInfo("float");
            gles_newparam.float2Attribute = gles_newparam.Type.GetAttributeInfo("float2");
            gles_newparam.float3Attribute = gles_newparam.Type.GetAttributeInfo("float3");
            gles_newparam.float4Attribute = gles_newparam.Type.GetAttributeInfo("float4");
            gles_newparam.float1x1Attribute = gles_newparam.Type.GetAttributeInfo("float1x1");
            gles_newparam.float1x2Attribute = gles_newparam.Type.GetAttributeInfo("float1x2");
            gles_newparam.float1x3Attribute = gles_newparam.Type.GetAttributeInfo("float1x3");
            gles_newparam.float1x4Attribute = gles_newparam.Type.GetAttributeInfo("float1x4");
            gles_newparam.float2x1Attribute = gles_newparam.Type.GetAttributeInfo("float2x1");
            gles_newparam.float2x2Attribute = gles_newparam.Type.GetAttributeInfo("float2x2");
            gles_newparam.float2x3Attribute = gles_newparam.Type.GetAttributeInfo("float2x3");
            gles_newparam.float2x4Attribute = gles_newparam.Type.GetAttributeInfo("float2x4");
            gles_newparam.float3x1Attribute = gles_newparam.Type.GetAttributeInfo("float3x1");
            gles_newparam.float3x2Attribute = gles_newparam.Type.GetAttributeInfo("float3x2");
            gles_newparam.float3x3Attribute = gles_newparam.Type.GetAttributeInfo("float3x3");
            gles_newparam.float3x4Attribute = gles_newparam.Type.GetAttributeInfo("float3x4");
            gles_newparam.float4x1Attribute = gles_newparam.Type.GetAttributeInfo("float4x1");
            gles_newparam.float4x2Attribute = gles_newparam.Type.GetAttributeInfo("float4x2");
            gles_newparam.float4x3Attribute = gles_newparam.Type.GetAttributeInfo("float4x3");
            gles_newparam.float4x4Attribute = gles_newparam.Type.GetAttributeInfo("float4x4");
            gles_newparam.enumAttribute = gles_newparam.Type.GetAttributeInfo("enum");
            gles_newparam.sidAttribute = gles_newparam.Type.GetAttributeInfo("sid");
            gles_newparam.annotateChild = gles_newparam.Type.GetChildInfo("annotate");
            gles_newparam.surfaceChild = gles_newparam.Type.GetChildInfo("surface");
            gles_newparam.texture_pipelineChild = gles_newparam.Type.GetChildInfo("texture_pipeline");
            gles_newparam.sampler_stateChild = gles_newparam.Type.GetChildInfo("sampler_state");
            gles_newparam.texture_unitChild = gles_newparam.Type.GetChildInfo("texture_unit");

            gles_texture_pipeline.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "gles_texture_pipeline");
            gles_texture_pipeline.sidAttribute = gles_texture_pipeline.Type.GetAttributeInfo("sid");
            gles_texture_pipeline.texcombinerChild = gles_texture_pipeline.Type.GetChildInfo("texcombiner");
            gles_texture_pipeline.texenvChild = gles_texture_pipeline.Type.GetChildInfo("texenv");
            gles_texture_pipeline.extraChild = gles_texture_pipeline.Type.GetChildInfo("extra");

            gles_texcombiner_command_type.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "gles_texcombiner_command_type");
            gles_texcombiner_command_type.constantChild = gles_texcombiner_command_type.Type.GetChildInfo("constant");
            gles_texcombiner_command_type.RGBChild = gles_texcombiner_command_type.Type.GetChildInfo("RGB");
            gles_texcombiner_command_type.alphaChild = gles_texcombiner_command_type.Type.GetChildInfo("alpha");

            gles_texture_constant_type.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "gles_texture_constant_type");
            gles_texture_constant_type.valueAttribute = gles_texture_constant_type.Type.GetAttributeInfo("value");
            gles_texture_constant_type.paramAttribute = gles_texture_constant_type.Type.GetAttributeInfo("param");

            gles_texcombiner_commandRGB_type.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "gles_texcombiner_commandRGB_type");
            gles_texcombiner_commandRGB_type.operatorAttribute = gles_texcombiner_commandRGB_type.Type.GetAttributeInfo("operator");
            gles_texcombiner_commandRGB_type.scaleAttribute = gles_texcombiner_commandRGB_type.Type.GetAttributeInfo("scale");
            gles_texcombiner_commandRGB_type.argumentChild = gles_texcombiner_commandRGB_type.Type.GetChildInfo("argument");

            gles_texcombiner_argumentRGB_type.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "gles_texcombiner_argumentRGB_type");
            gles_texcombiner_argumentRGB_type.sourceAttribute = gles_texcombiner_argumentRGB_type.Type.GetAttributeInfo("source");
            gles_texcombiner_argumentRGB_type.operandAttribute = gles_texcombiner_argumentRGB_type.Type.GetAttributeInfo("operand");
            gles_texcombiner_argumentRGB_type.unitAttribute = gles_texcombiner_argumentRGB_type.Type.GetAttributeInfo("unit");

            gles_texcombiner_commandAlpha_type.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "gles_texcombiner_commandAlpha_type");
            gles_texcombiner_commandAlpha_type.operatorAttribute = gles_texcombiner_commandAlpha_type.Type.GetAttributeInfo("operator");
            gles_texcombiner_commandAlpha_type.scaleAttribute = gles_texcombiner_commandAlpha_type.Type.GetAttributeInfo("scale");
            gles_texcombiner_commandAlpha_type.argumentChild = gles_texcombiner_commandAlpha_type.Type.GetChildInfo("argument");

            gles_texcombiner_argumentAlpha_type.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "gles_texcombiner_argumentAlpha_type");
            gles_texcombiner_argumentAlpha_type.sourceAttribute = gles_texcombiner_argumentAlpha_type.Type.GetAttributeInfo("source");
            gles_texcombiner_argumentAlpha_type.operandAttribute = gles_texcombiner_argumentAlpha_type.Type.GetAttributeInfo("operand");
            gles_texcombiner_argumentAlpha_type.unitAttribute = gles_texcombiner_argumentAlpha_type.Type.GetAttributeInfo("unit");

            gles_texenv_command_type.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "gles_texenv_command_type");
            gles_texenv_command_type.operatorAttribute = gles_texenv_command_type.Type.GetAttributeInfo("operator");
            gles_texenv_command_type.unitAttribute = gles_texenv_command_type.Type.GetAttributeInfo("unit");
            gles_texenv_command_type.constantChild = gles_texenv_command_type.Type.GetChildInfo("constant");

            gles_sampler_state.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "gles_sampler_state");
            gles_sampler_state.wrap_sAttribute = gles_sampler_state.Type.GetAttributeInfo("wrap_s");
            gles_sampler_state.wrap_tAttribute = gles_sampler_state.Type.GetAttributeInfo("wrap_t");
            gles_sampler_state.minfilterAttribute = gles_sampler_state.Type.GetAttributeInfo("minfilter");
            gles_sampler_state.magfilterAttribute = gles_sampler_state.Type.GetAttributeInfo("magfilter");
            gles_sampler_state.mipfilterAttribute = gles_sampler_state.Type.GetAttributeInfo("mipfilter");
            gles_sampler_state.mipmap_maxlevelAttribute = gles_sampler_state.Type.GetAttributeInfo("mipmap_maxlevel");
            gles_sampler_state.mipmap_biasAttribute = gles_sampler_state.Type.GetAttributeInfo("mipmap_bias");
            gles_sampler_state.sidAttribute = gles_sampler_state.Type.GetAttributeInfo("sid");
            gles_sampler_state.extraChild = gles_sampler_state.Type.GetChildInfo("extra");

            gles_texture_unit.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "gles_texture_unit");
            gles_texture_unit.surfaceAttribute = gles_texture_unit.Type.GetAttributeInfo("surface");
            gles_texture_unit.sampler_stateAttribute = gles_texture_unit.Type.GetAttributeInfo("sampler_state");
            gles_texture_unit.sidAttribute = gles_texture_unit.Type.GetAttributeInfo("sid");
            gles_texture_unit.texcoordChild = gles_texture_unit.Type.GetChildInfo("texcoord");
            gles_texture_unit.extraChild = gles_texture_unit.Type.GetChildInfo("extra");

            gles_texture_unit_texcoord.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "gles_texture_unit_texcoord");
            gles_texture_unit_texcoord.semanticAttribute = gles_texture_unit_texcoord.Type.GetAttributeInfo("semantic");

            profile_GLES_technique.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "profile_GLES_technique");
            profile_GLES_technique.idAttribute = profile_GLES_technique.Type.GetAttributeInfo("id");
            profile_GLES_technique.sidAttribute = profile_GLES_technique.Type.GetAttributeInfo("sid");
            profile_GLES_technique.assetChild = profile_GLES_technique.Type.GetChildInfo("asset");
            profile_GLES_technique.annotateChild = profile_GLES_technique.Type.GetChildInfo("annotate");
            profile_GLES_technique.imageChild = profile_GLES_technique.Type.GetChildInfo("image");
            profile_GLES_technique.newparamChild = profile_GLES_technique.Type.GetChildInfo("newparam");
            profile_GLES_technique.setparamChild = profile_GLES_technique.Type.GetChildInfo("setparam");
            profile_GLES_technique.passChild = profile_GLES_technique.Type.GetChildInfo("pass");
            profile_GLES_technique.extraChild = profile_GLES_technique.Type.GetChildInfo("extra");

            technique_setparam.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "technique_setparam");
            technique_setparam.boolAttribute = technique_setparam.Type.GetAttributeInfo("bool");
            technique_setparam.bool2Attribute = technique_setparam.Type.GetAttributeInfo("bool2");
            technique_setparam.bool3Attribute = technique_setparam.Type.GetAttributeInfo("bool3");
            technique_setparam.bool4Attribute = technique_setparam.Type.GetAttributeInfo("bool4");
            technique_setparam.intAttribute = technique_setparam.Type.GetAttributeInfo("int");
            technique_setparam.int2Attribute = technique_setparam.Type.GetAttributeInfo("int2");
            technique_setparam.int3Attribute = technique_setparam.Type.GetAttributeInfo("int3");
            technique_setparam.int4Attribute = technique_setparam.Type.GetAttributeInfo("int4");
            technique_setparam.floatAttribute = technique_setparam.Type.GetAttributeInfo("float");
            technique_setparam.float2Attribute = technique_setparam.Type.GetAttributeInfo("float2");
            technique_setparam.float3Attribute = technique_setparam.Type.GetAttributeInfo("float3");
            technique_setparam.float4Attribute = technique_setparam.Type.GetAttributeInfo("float4");
            technique_setparam.float1x1Attribute = technique_setparam.Type.GetAttributeInfo("float1x1");
            technique_setparam.float1x2Attribute = technique_setparam.Type.GetAttributeInfo("float1x2");
            technique_setparam.float1x3Attribute = technique_setparam.Type.GetAttributeInfo("float1x3");
            technique_setparam.float1x4Attribute = technique_setparam.Type.GetAttributeInfo("float1x4");
            technique_setparam.float2x1Attribute = technique_setparam.Type.GetAttributeInfo("float2x1");
            technique_setparam.float2x2Attribute = technique_setparam.Type.GetAttributeInfo("float2x2");
            technique_setparam.float2x3Attribute = technique_setparam.Type.GetAttributeInfo("float2x3");
            technique_setparam.float2x4Attribute = technique_setparam.Type.GetAttributeInfo("float2x4");
            technique_setparam.float3x1Attribute = technique_setparam.Type.GetAttributeInfo("float3x1");
            technique_setparam.float3x2Attribute = technique_setparam.Type.GetAttributeInfo("float3x2");
            technique_setparam.float3x3Attribute = technique_setparam.Type.GetAttributeInfo("float3x3");
            technique_setparam.float3x4Attribute = technique_setparam.Type.GetAttributeInfo("float3x4");
            technique_setparam.float4x1Attribute = technique_setparam.Type.GetAttributeInfo("float4x1");
            technique_setparam.float4x2Attribute = technique_setparam.Type.GetAttributeInfo("float4x2");
            technique_setparam.float4x3Attribute = technique_setparam.Type.GetAttributeInfo("float4x3");
            technique_setparam.float4x4Attribute = technique_setparam.Type.GetAttributeInfo("float4x4");
            technique_setparam.enumAttribute = technique_setparam.Type.GetAttributeInfo("enum");
            technique_setparam.refAttribute = technique_setparam.Type.GetAttributeInfo("ref");
            technique_setparam.annotateChild = technique_setparam.Type.GetChildInfo("annotate");
            technique_setparam.surfaceChild = technique_setparam.Type.GetChildInfo("surface");
            technique_setparam.texture_pipelineChild = technique_setparam.Type.GetChildInfo("texture_pipeline");
            technique_setparam.sampler_stateChild = technique_setparam.Type.GetChildInfo("sampler_state");
            technique_setparam.texture_unitChild = technique_setparam.Type.GetChildInfo("texture_unit");

            profile_GLES_technique_pass.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "profile_GLES_technique_pass");
            profile_GLES_technique_pass.color_targetAttribute = profile_GLES_technique_pass.Type.GetAttributeInfo("color_target");
            profile_GLES_technique_pass.depth_targetAttribute = profile_GLES_technique_pass.Type.GetAttributeInfo("depth_target");
            profile_GLES_technique_pass.stencil_targetAttribute = profile_GLES_technique_pass.Type.GetAttributeInfo("stencil_target");
            profile_GLES_technique_pass.color_clearAttribute = profile_GLES_technique_pass.Type.GetAttributeInfo("color_clear");
            profile_GLES_technique_pass.depth_clearAttribute = profile_GLES_technique_pass.Type.GetAttributeInfo("depth_clear");
            profile_GLES_technique_pass.stencil_clearAttribute = profile_GLES_technique_pass.Type.GetAttributeInfo("stencil_clear");
            profile_GLES_technique_pass.drawAttribute = profile_GLES_technique_pass.Type.GetAttributeInfo("draw");
            profile_GLES_technique_pass.sidAttribute = profile_GLES_technique_pass.Type.GetAttributeInfo("sid");
            profile_GLES_technique_pass.annotateChild = profile_GLES_technique_pass.Type.GetChildInfo("annotate");
            profile_GLES_technique_pass.alpha_funcChild = profile_GLES_technique_pass.Type.GetChildInfo("alpha_func");
            profile_GLES_technique_pass.blend_funcChild = profile_GLES_technique_pass.Type.GetChildInfo("blend_func");
            profile_GLES_technique_pass.clear_colorChild = profile_GLES_technique_pass.Type.GetChildInfo("clear_color");
            profile_GLES_technique_pass.clear_stencilChild = profile_GLES_technique_pass.Type.GetChildInfo("clear_stencil");
            profile_GLES_technique_pass.clear_depthChild = profile_GLES_technique_pass.Type.GetChildInfo("clear_depth");
            profile_GLES_technique_pass.clip_planeChild = profile_GLES_technique_pass.Type.GetChildInfo("clip_plane");
            profile_GLES_technique_pass.color_maskChild = profile_GLES_technique_pass.Type.GetChildInfo("color_mask");
            profile_GLES_technique_pass.cull_faceChild = profile_GLES_technique_pass.Type.GetChildInfo("cull_face");
            profile_GLES_technique_pass.depth_funcChild = profile_GLES_technique_pass.Type.GetChildInfo("depth_func");
            profile_GLES_technique_pass.depth_maskChild = profile_GLES_technique_pass.Type.GetChildInfo("depth_mask");
            profile_GLES_technique_pass.depth_rangeChild = profile_GLES_technique_pass.Type.GetChildInfo("depth_range");
            profile_GLES_technique_pass.fog_colorChild = profile_GLES_technique_pass.Type.GetChildInfo("fog_color");
            profile_GLES_technique_pass.fog_densityChild = profile_GLES_technique_pass.Type.GetChildInfo("fog_density");
            profile_GLES_technique_pass.fog_modeChild = profile_GLES_technique_pass.Type.GetChildInfo("fog_mode");
            profile_GLES_technique_pass.fog_startChild = profile_GLES_technique_pass.Type.GetChildInfo("fog_start");
            profile_GLES_technique_pass.fog_endChild = profile_GLES_technique_pass.Type.GetChildInfo("fog_end");
            profile_GLES_technique_pass.front_faceChild = profile_GLES_technique_pass.Type.GetChildInfo("front_face");
            profile_GLES_technique_pass.texture_pipelineChild = profile_GLES_technique_pass.Type.GetChildInfo("texture_pipeline");
            profile_GLES_technique_pass.logic_opChild = profile_GLES_technique_pass.Type.GetChildInfo("logic_op");
            profile_GLES_technique_pass.light_ambientChild = profile_GLES_technique_pass.Type.GetChildInfo("light_ambient");
            profile_GLES_technique_pass.light_diffuseChild = profile_GLES_technique_pass.Type.GetChildInfo("light_diffuse");
            profile_GLES_technique_pass.light_specularChild = profile_GLES_technique_pass.Type.GetChildInfo("light_specular");
            profile_GLES_technique_pass.light_positionChild = profile_GLES_technique_pass.Type.GetChildInfo("light_position");
            profile_GLES_technique_pass.light_constant_attenuationChild = profile_GLES_technique_pass.Type.GetChildInfo("light_constant_attenuation");
            profile_GLES_technique_pass.light_linear_attenutationChild = profile_GLES_technique_pass.Type.GetChildInfo("light_linear_attenutation");
            profile_GLES_technique_pass.light_quadratic_attenuationChild = profile_GLES_technique_pass.Type.GetChildInfo("light_quadratic_attenuation");
            profile_GLES_technique_pass.light_spot_cutoffChild = profile_GLES_technique_pass.Type.GetChildInfo("light_spot_cutoff");
            profile_GLES_technique_pass.light_spot_directionChild = profile_GLES_technique_pass.Type.GetChildInfo("light_spot_direction");
            profile_GLES_technique_pass.light_spot_exponentChild = profile_GLES_technique_pass.Type.GetChildInfo("light_spot_exponent");
            profile_GLES_technique_pass.light_model_ambientChild = profile_GLES_technique_pass.Type.GetChildInfo("light_model_ambient");
            profile_GLES_technique_pass.line_widthChild = profile_GLES_technique_pass.Type.GetChildInfo("line_width");
            profile_GLES_technique_pass.material_ambientChild = profile_GLES_technique_pass.Type.GetChildInfo("material_ambient");
            profile_GLES_technique_pass.material_diffuseChild = profile_GLES_technique_pass.Type.GetChildInfo("material_diffuse");
            profile_GLES_technique_pass.material_emissionChild = profile_GLES_technique_pass.Type.GetChildInfo("material_emission");
            profile_GLES_technique_pass.material_shininessChild = profile_GLES_technique_pass.Type.GetChildInfo("material_shininess");
            profile_GLES_technique_pass.material_specularChild = profile_GLES_technique_pass.Type.GetChildInfo("material_specular");
            profile_GLES_technique_pass.model_view_matrixChild = profile_GLES_technique_pass.Type.GetChildInfo("model_view_matrix");
            profile_GLES_technique_pass.point_distance_attenuationChild = profile_GLES_technique_pass.Type.GetChildInfo("point_distance_attenuation");
            profile_GLES_technique_pass.point_fade_threshold_sizeChild = profile_GLES_technique_pass.Type.GetChildInfo("point_fade_threshold_size");
            profile_GLES_technique_pass.point_sizeChild = profile_GLES_technique_pass.Type.GetChildInfo("point_size");
            profile_GLES_technique_pass.point_size_minChild = profile_GLES_technique_pass.Type.GetChildInfo("point_size_min");
            profile_GLES_technique_pass.point_size_maxChild = profile_GLES_technique_pass.Type.GetChildInfo("point_size_max");
            profile_GLES_technique_pass.polygon_offsetChild = profile_GLES_technique_pass.Type.GetChildInfo("polygon_offset");
            profile_GLES_technique_pass.projection_matrixChild = profile_GLES_technique_pass.Type.GetChildInfo("projection_matrix");
            profile_GLES_technique_pass.scissorChild = profile_GLES_technique_pass.Type.GetChildInfo("scissor");
            profile_GLES_technique_pass.shade_modelChild = profile_GLES_technique_pass.Type.GetChildInfo("shade_model");
            profile_GLES_technique_pass.stencil_funcChild = profile_GLES_technique_pass.Type.GetChildInfo("stencil_func");
            profile_GLES_technique_pass.stencil_maskChild = profile_GLES_technique_pass.Type.GetChildInfo("stencil_mask");
            profile_GLES_technique_pass.stencil_opChild = profile_GLES_technique_pass.Type.GetChildInfo("stencil_op");
            profile_GLES_technique_pass.alpha_test_enableChild = profile_GLES_technique_pass.Type.GetChildInfo("alpha_test_enable");
            profile_GLES_technique_pass.blend_enableChild = profile_GLES_technique_pass.Type.GetChildInfo("blend_enable");
            profile_GLES_technique_pass.clip_plane_enableChild = profile_GLES_technique_pass.Type.GetChildInfo("clip_plane_enable");
            profile_GLES_technique_pass.color_logic_op_enableChild = profile_GLES_technique_pass.Type.GetChildInfo("color_logic_op_enable");
            profile_GLES_technique_pass.color_material_enableChild = profile_GLES_technique_pass.Type.GetChildInfo("color_material_enable");
            profile_GLES_technique_pass.cull_face_enableChild = profile_GLES_technique_pass.Type.GetChildInfo("cull_face_enable");
            profile_GLES_technique_pass.depth_test_enableChild = profile_GLES_technique_pass.Type.GetChildInfo("depth_test_enable");
            profile_GLES_technique_pass.dither_enableChild = profile_GLES_technique_pass.Type.GetChildInfo("dither_enable");
            profile_GLES_technique_pass.fog_enableChild = profile_GLES_technique_pass.Type.GetChildInfo("fog_enable");
            profile_GLES_technique_pass.texture_pipeline_enableChild = profile_GLES_technique_pass.Type.GetChildInfo("texture_pipeline_enable");
            profile_GLES_technique_pass.light_enableChild = profile_GLES_technique_pass.Type.GetChildInfo("light_enable");
            profile_GLES_technique_pass.lighting_enableChild = profile_GLES_technique_pass.Type.GetChildInfo("lighting_enable");
            profile_GLES_technique_pass.light_model_two_side_enableChild = profile_GLES_technique_pass.Type.GetChildInfo("light_model_two_side_enable");
            profile_GLES_technique_pass.line_smooth_enableChild = profile_GLES_technique_pass.Type.GetChildInfo("line_smooth_enable");
            profile_GLES_technique_pass.multisample_enableChild = profile_GLES_technique_pass.Type.GetChildInfo("multisample_enable");
            profile_GLES_technique_pass.normalize_enableChild = profile_GLES_technique_pass.Type.GetChildInfo("normalize_enable");
            profile_GLES_technique_pass.point_smooth_enableChild = profile_GLES_technique_pass.Type.GetChildInfo("point_smooth_enable");
            profile_GLES_technique_pass.polygon_offset_fill_enableChild = profile_GLES_technique_pass.Type.GetChildInfo("polygon_offset_fill_enable");
            profile_GLES_technique_pass.rescale_normal_enableChild = profile_GLES_technique_pass.Type.GetChildInfo("rescale_normal_enable");
            profile_GLES_technique_pass.sample_alpha_to_coverage_enableChild = profile_GLES_technique_pass.Type.GetChildInfo("sample_alpha_to_coverage_enable");
            profile_GLES_technique_pass.sample_alpha_to_one_enableChild = profile_GLES_technique_pass.Type.GetChildInfo("sample_alpha_to_one_enable");
            profile_GLES_technique_pass.sample_coverage_enableChild = profile_GLES_technique_pass.Type.GetChildInfo("sample_coverage_enable");
            profile_GLES_technique_pass.scissor_test_enableChild = profile_GLES_technique_pass.Type.GetChildInfo("scissor_test_enable");
            profile_GLES_technique_pass.stencil_test_enableChild = profile_GLES_technique_pass.Type.GetChildInfo("stencil_test_enable");
            profile_GLES_technique_pass.extraChild = profile_GLES_technique_pass.Type.GetChildInfo("extra");

            texture_pipeline.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "texture_pipeline");
            texture_pipeline.paramAttribute = texture_pipeline.Type.GetAttributeInfo("param");
            texture_pipeline.valueChild = texture_pipeline.Type.GetChildInfo("value");

            light_linear_attenutation.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "light_linear_attenutation");
            light_linear_attenutation.valueAttribute = light_linear_attenutation.Type.GetAttributeInfo("value");
            light_linear_attenutation.paramAttribute = light_linear_attenutation.Type.GetAttributeInfo("param");
            light_linear_attenutation.indexAttribute = light_linear_attenutation.Type.GetAttributeInfo("index");

            texture_pipeline_enable.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "texture_pipeline_enable");
            texture_pipeline_enable.valueAttribute = texture_pipeline_enable.Type.GetAttributeInfo("value");
            texture_pipeline_enable.paramAttribute = texture_pipeline_enable.Type.GetAttributeInfo("param");

            ellipsoid.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "ellipsoid");
            ellipsoid.sizeAttribute = ellipsoid.Type.GetAttributeInfo("size");

            InputGlobal.Type = getNodeType("http://www.collada.org/2005/11/COLLADASchema", "InputGlobal");
            InputGlobal.semanticAttribute = InputGlobal.Type.GetAttributeInfo("semantic");
            InputGlobal.sourceAttribute = InputGlobal.Type.GetAttributeInfo("source");

            COLLADARootElement = getRootElement(NS, "COLLADA");
            IDREF_arrayRootElement = getRootElement(NS, "IDREF_array");
            Name_arrayRootElement = getRootElement(NS, "Name_array");
            bool_arrayRootElement = getRootElement(NS, "bool_array");
            float_arrayRootElement = getRootElement(NS, "float_array");
            int_arrayRootElement = getRootElement(NS, "int_array");
            accessorRootElement = getRootElement(NS, "accessor");
            paramRootElement = getRootElement(NS, "param");
            sourceRootElement = getRootElement(NS, "source");
            geometryRootElement = getRootElement(NS, "geometry");
            meshRootElement = getRootElement(NS, "mesh");
            splineRootElement = getRootElement(NS, "spline");
            pRootElement = getRootElement(NS, "p");
            linesRootElement = getRootElement(NS, "lines");
            linestripsRootElement = getRootElement(NS, "linestrips");
            polygonsRootElement = getRootElement(NS, "polygons");
            polylistRootElement = getRootElement(NS, "polylist");
            trianglesRootElement = getRootElement(NS, "triangles");
            trifansRootElement = getRootElement(NS, "trifans");
            tristripsRootElement = getRootElement(NS, "tristrips");
            verticesRootElement = getRootElement(NS, "vertices");
            lookatRootElement = getRootElement(NS, "lookat");
            matrixRootElement = getRootElement(NS, "matrix");
            rotateRootElement = getRootElement(NS, "rotate");
            scaleRootElement = getRootElement(NS, "scale");
            skewRootElement = getRootElement(NS, "skew");
            translateRootElement = getRootElement(NS, "translate");
            imageRootElement = getRootElement(NS, "image");
            lightRootElement = getRootElement(NS, "light");
            materialRootElement = getRootElement(NS, "material");
            cameraRootElement = getRootElement(NS, "camera");
            animationRootElement = getRootElement(NS, "animation");
            animation_clipRootElement = getRootElement(NS, "animation_clip");
            channelRootElement = getRootElement(NS, "channel");
            samplerRootElement = getRootElement(NS, "sampler");
            controllerRootElement = getRootElement(NS, "controller");
            skinRootElement = getRootElement(NS, "skin");
            morphRootElement = getRootElement(NS, "morph");
            assetRootElement = getRootElement(NS, "asset");
            extraRootElement = getRootElement(NS, "extra");
            techniqueRootElement = getRootElement(NS, "technique");
            nodeRootElement = getRootElement(NS, "node");
            visual_sceneRootElement = getRootElement(NS, "visual_scene");
            bind_materialRootElement = getRootElement(NS, "bind_material");
            instance_cameraRootElement = getRootElement(NS, "instance_camera");
            instance_controllerRootElement = getRootElement(NS, "instance_controller");
            instance_effectRootElement = getRootElement(NS, "instance_effect");
            instance_force_fieldRootElement = getRootElement(NS, "instance_force_field");
            instance_geometryRootElement = getRootElement(NS, "instance_geometry");
            instance_lightRootElement = getRootElement(NS, "instance_light");
            instance_materialRootElement = getRootElement(NS, "instance_material");
            instance_nodeRootElement = getRootElement(NS, "instance_node");
            instance_physics_materialRootElement = getRootElement(NS, "instance_physics_material");
            instance_physics_modelRootElement = getRootElement(NS, "instance_physics_model");
            instance_rigid_bodyRootElement = getRootElement(NS, "instance_rigid_body");
            instance_rigid_constraintRootElement = getRootElement(NS, "instance_rigid_constraint");
            library_animationsRootElement = getRootElement(NS, "library_animations");
            library_animation_clipsRootElement = getRootElement(NS, "library_animation_clips");
            library_camerasRootElement = getRootElement(NS, "library_cameras");
            library_controllersRootElement = getRootElement(NS, "library_controllers");
            library_geometriesRootElement = getRootElement(NS, "library_geometries");
            library_effectsRootElement = getRootElement(NS, "library_effects");
            library_force_fieldsRootElement = getRootElement(NS, "library_force_fields");
            library_imagesRootElement = getRootElement(NS, "library_images");
            library_lightsRootElement = getRootElement(NS, "library_lights");
            library_materialsRootElement = getRootElement(NS, "library_materials");
            library_nodesRootElement = getRootElement(NS, "library_nodes");
            library_physics_materialsRootElement = getRootElement(NS, "library_physics_materials");
            library_physics_modelsRootElement = getRootElement(NS, "library_physics_models");
            library_physics_scenesRootElement = getRootElement(NS, "library_physics_scenes");
            library_visual_scenesRootElement = getRootElement(NS, "library_visual_scenes");
            fx_profile_abstractRootElement = getRootElement(NS, "fx_profile_abstract");
            effectRootElement = getRootElement(NS, "effect");
            gl_hook_abstractRootElement = getRootElement(NS, "gl_hook_abstract");
            profile_GLSLRootElement = getRootElement(NS, "profile_GLSL");
            profile_COMMONRootElement = getRootElement(NS, "profile_COMMON");
            profile_CGRootElement = getRootElement(NS, "profile_CG");
            profile_GLESRootElement = getRootElement(NS, "profile_GLES");
            boxRootElement = getRootElement(NS, "box");
            planeRootElement = getRootElement(NS, "plane");
            sphereRootElement = getRootElement(NS, "sphere");
            ellipsoidRootElement = getRootElement(NS, "ellipsoid");
            cylinderRootElement = getRootElement(NS, "cylinder");
            tapered_cylinderRootElement = getRootElement(NS, "tapered_cylinder");
            capsuleRootElement = getRootElement(NS, "capsule");
            tapered_capsuleRootElement = getRootElement(NS, "tapered_capsule");
            convex_meshRootElement = getRootElement(NS, "convex_mesh");
            force_fieldRootElement = getRootElement(NS, "force_field");
            physics_materialRootElement = getRootElement(NS, "physics_material");
            physics_sceneRootElement = getRootElement(NS, "physics_scene");
            rigid_bodyRootElement = getRootElement(NS, "rigid_body");
            rigid_constraintRootElement = getRootElement(NS, "rigid_constraint");
            physics_modelRootElement = getRootElement(NS, "physics_model");
        }

        public static class COLLADA
        {
            public static DomNodeType Type;
            public static AttributeInfo versionAttribute;
            public static ChildInfo assetChild;
            public static ChildInfo library_animationsChild;
            public static ChildInfo library_animation_clipsChild;
            public static ChildInfo library_camerasChild;
            public static ChildInfo library_controllersChild;
            public static ChildInfo library_geometriesChild;
            public static ChildInfo library_effectsChild;
            public static ChildInfo library_force_fieldsChild;
            public static ChildInfo library_imagesChild;
            public static ChildInfo library_lightsChild;
            public static ChildInfo library_materialsChild;
            public static ChildInfo library_nodesChild;
            public static ChildInfo library_physics_materialsChild;
            public static ChildInfo library_physics_modelsChild;
            public static ChildInfo library_physics_scenesChild;
            public static ChildInfo library_visual_scenesChild;
            public static ChildInfo sceneChild;
            public static ChildInfo extraChild;
        }

        public static class asset
        {
            public static DomNodeType Type;
            public static AttributeInfo createdAttribute;
            public static AttributeInfo keywordsAttribute;
            public static AttributeInfo modifiedAttribute;
            public static AttributeInfo revisionAttribute;
            public static AttributeInfo subjectAttribute;
            public static AttributeInfo titleAttribute;
            public static AttributeInfo up_axisAttribute;
            public static ChildInfo contributorChild;
            public static ChildInfo unitChild;
        }

        public static class asset_contributor
        {
            public static DomNodeType Type;
            public static AttributeInfo authorAttribute;
            public static AttributeInfo authoring_toolAttribute;
            public static AttributeInfo commentsAttribute;
            public static AttributeInfo copyrightAttribute;
            public static AttributeInfo source_dataAttribute;
        }

        public static class asset_unit
        {
            public static DomNodeType Type;
            public static AttributeInfo meterAttribute;
            public static AttributeInfo nameAttribute;
        }

        public static class library_animations
        {
            public static DomNodeType Type;
            public static AttributeInfo idAttribute;
            public static AttributeInfo nameAttribute;
            public static ChildInfo assetChild;
            public static ChildInfo animationChild;
            public static ChildInfo extraChild;
        }

        public static class animation
        {
            public static DomNodeType Type;
            public static AttributeInfo idAttribute;
            public static AttributeInfo nameAttribute;
            public static ChildInfo assetChild;
            public static ChildInfo sourceChild;
            public static ChildInfo samplerChild;
            public static ChildInfo channelChild;
            public static ChildInfo animationChild;
            public static ChildInfo extraChild;
        }

        public static class source
        {
            public static DomNodeType Type;
            public static AttributeInfo idAttribute;
            public static AttributeInfo nameAttribute;
            public static ChildInfo assetChild;
            public static ChildInfo IDREF_arrayChild;
            public static ChildInfo Name_arrayChild;
            public static ChildInfo bool_arrayChild;
            public static ChildInfo float_arrayChild;
            public static ChildInfo int_arrayChild;
            public static ChildInfo technique_commonChild;
            public static ChildInfo techniqueChild;
        }

        public static class IDREF_array
        {
            public static DomNodeType Type;
            public static AttributeInfo Attribute;
            public static AttributeInfo idAttribute;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo countAttribute;
        }

        public static class Name_array
        {
            public static DomNodeType Type;
            public static AttributeInfo Attribute;
            public static AttributeInfo idAttribute;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo countAttribute;
        }

        public static class bool_array
        {
            public static DomNodeType Type;
            public static AttributeInfo Attribute;
            public static AttributeInfo idAttribute;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo countAttribute;
        }

        public static class float_array
        {
            public static DomNodeType Type;
            public static AttributeInfo Attribute;
            public static AttributeInfo idAttribute;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo countAttribute;
            public static AttributeInfo digitsAttribute;
            public static AttributeInfo magnitudeAttribute;
        }

        public static class int_array
        {
            public static DomNodeType Type;
            public static AttributeInfo Attribute;
            public static AttributeInfo idAttribute;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo countAttribute;
            public static AttributeInfo minInclusiveAttribute;
            public static AttributeInfo maxInclusiveAttribute;
        }

        public static class source_technique_common
        {
            public static DomNodeType Type;
            public static ChildInfo accessorChild;
        }

        public static class accessor
        {
            public static DomNodeType Type;
            public static AttributeInfo countAttribute;
            public static AttributeInfo offsetAttribute;
            public static AttributeInfo sourceAttribute;
            public static AttributeInfo strideAttribute;
            public static ChildInfo paramChild;
        }

        public static class param
        {
            public static DomNodeType Type;
            public static AttributeInfo Attribute;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo sidAttribute;
            public static AttributeInfo semanticAttribute;
            public static AttributeInfo typeAttribute;
        }

        public static class technique
        {
            public static DomNodeType Type;
            public static AttributeInfo profileAttribute;
        }

        public static class sampler
        {
            public static DomNodeType Type;
            public static AttributeInfo idAttribute;
            public static ChildInfo inputChild;
        }

        public static class InputLocal
        {
            public static DomNodeType Type;
            public static AttributeInfo semanticAttribute;
            public static AttributeInfo sourceAttribute;
        }

        public static class channel
        {
            public static DomNodeType Type;
            public static AttributeInfo sourceAttribute;
            public static AttributeInfo targetAttribute;
        }

        public static class extra
        {
            public static DomNodeType Type;
            public static AttributeInfo idAttribute;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo typeAttribute;
            public static ChildInfo assetChild;
            public static ChildInfo techniqueChild;
        }

        public static class library_animation_clips
        {
            public static DomNodeType Type;
            public static AttributeInfo idAttribute;
            public static AttributeInfo nameAttribute;
            public static ChildInfo assetChild;
            public static ChildInfo animation_clipChild;
            public static ChildInfo extraChild;
        }

        public static class animation_clip
        {
            public static DomNodeType Type;
            public static AttributeInfo idAttribute;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo startAttribute;
            public static AttributeInfo endAttribute;
            public static ChildInfo assetChild;
            public static ChildInfo instance_animationChild;
            public static ChildInfo extraChild;
        }

        public static class InstanceWithExtra
        {
            public static DomNodeType Type;
            public static AttributeInfo urlAttribute;
            public static AttributeInfo sidAttribute;
            public static AttributeInfo nameAttribute;
            public static ChildInfo extraChild;
        }

        public static class library_cameras
        {
            public static DomNodeType Type;
            public static AttributeInfo idAttribute;
            public static AttributeInfo nameAttribute;
            public static ChildInfo assetChild;
            public static ChildInfo cameraChild;
            public static ChildInfo extraChild;
        }

        public static class camera
        {
            public static DomNodeType Type;
            public static AttributeInfo idAttribute;
            public static AttributeInfo nameAttribute;
            public static ChildInfo assetChild;
            public static ChildInfo opticsChild;
            public static ChildInfo imagerChild;
            public static ChildInfo extraChild;
        }

        public static class camera_optics
        {
            public static DomNodeType Type;
            public static ChildInfo technique_commonChild;
            public static ChildInfo techniqueChild;
            public static ChildInfo extraChild;
        }

        public static class optics_technique_common
        {
            public static DomNodeType Type;
            public static ChildInfo orthographicChild;
            public static ChildInfo perspectiveChild;
        }

        public static class technique_common_orthographic
        {
            public static DomNodeType Type;
            public static ChildInfo xmagChild;
            public static ChildInfo ymagChild;
            public static ChildInfo aspect_ratioChild;
            public static ChildInfo znearChild;
            public static ChildInfo zfarChild;
        }

        public static class TargetableFloat
        {
            public static DomNodeType Type;
            public static AttributeInfo Attribute;
            public static AttributeInfo sidAttribute;
        }

        public static class technique_common_perspective
        {
            public static DomNodeType Type;
            public static ChildInfo xfovChild;
            public static ChildInfo yfovChild;
            public static ChildInfo aspect_ratioChild;
            public static ChildInfo znearChild;
            public static ChildInfo zfarChild;
        }

        public static class camera_imager
        {
            public static DomNodeType Type;
            public static ChildInfo techniqueChild;
            public static ChildInfo extraChild;
        }

        public static class library_controllers
        {
            public static DomNodeType Type;
            public static AttributeInfo idAttribute;
            public static AttributeInfo nameAttribute;
            public static ChildInfo assetChild;
            public static ChildInfo controllerChild;
            public static ChildInfo extraChild;
        }

        public static class controller
        {
            public static DomNodeType Type;
            public static AttributeInfo idAttribute;
            public static AttributeInfo nameAttribute;
            public static ChildInfo assetChild;
            public static ChildInfo skinChild;
            public static ChildInfo morphChild;
            public static ChildInfo extraChild;
        }

        public static class skin
        {
            public static DomNodeType Type;
            public static AttributeInfo bind_shape_matrixAttribute;
            public static AttributeInfo sourceAttribute;
            public static ChildInfo sourceChild;
            public static ChildInfo jointsChild;
            public static ChildInfo vertex_weightsChild;
            public static ChildInfo extraChild;
        }

        public static class skin_joints
        {
            public static DomNodeType Type;
            public static ChildInfo inputChild;
            public static ChildInfo extraChild;
        }

        public static class skin_vertex_weights
        {
            public static DomNodeType Type;
            public static AttributeInfo vcountAttribute;
            public static AttributeInfo vAttribute;
            public static AttributeInfo countAttribute;
            public static ChildInfo inputChild;
            public static ChildInfo extraChild;
        }

        public static class InputLocalOffset
        {
            public static DomNodeType Type;
            public static AttributeInfo offsetAttribute;
            public static AttributeInfo semanticAttribute;
            public static AttributeInfo sourceAttribute;
            public static AttributeInfo setAttribute;
        }

        public static class morph
        {
            public static DomNodeType Type;
            public static AttributeInfo methodAttribute;
            public static AttributeInfo sourceAttribute;
            public static ChildInfo sourceChild;
            public static ChildInfo targetsChild;
            public static ChildInfo extraChild;
        }

        public static class morph_targets
        {
            public static DomNodeType Type;
            public static ChildInfo inputChild;
            public static ChildInfo extraChild;
        }

        public static class library_geometries
        {
            public static DomNodeType Type;
            public static AttributeInfo idAttribute;
            public static AttributeInfo nameAttribute;
            public static ChildInfo assetChild;
            public static ChildInfo geometryChild;
            public static ChildInfo extraChild;
        }

        public static class geometry
        {
            public static DomNodeType Type;
            public static AttributeInfo idAttribute;
            public static AttributeInfo nameAttribute;
            public static ChildInfo assetChild;
            public static ChildInfo convex_meshChild;
            public static ChildInfo meshChild;
            public static ChildInfo splineChild;
            public static ChildInfo extraChild;
        }

        public static class convex_mesh
        {
            public static DomNodeType Type;
            public static AttributeInfo convex_hull_ofAttribute;
            public static ChildInfo sourceChild;
            public static ChildInfo verticesChild;
            public static ChildInfo linesChild;
            public static ChildInfo linestripsChild;
            public static ChildInfo polygonsChild;
            public static ChildInfo polylistChild;
            public static ChildInfo trianglesChild;
            public static ChildInfo trifansChild;
            public static ChildInfo tristripsChild;
            public static ChildInfo extraChild;
        }

        public static class vertices
        {
            public static DomNodeType Type;
            public static AttributeInfo idAttribute;
            public static AttributeInfo nameAttribute;
            public static ChildInfo inputChild;
            public static ChildInfo extraChild;
        }

        public static class lines
        {
            public static DomNodeType Type;
            public static AttributeInfo pAttribute;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo countAttribute;
            public static AttributeInfo materialAttribute;
            public static ChildInfo inputChild;
            public static ChildInfo extraChild;
        }

        public static class linestrips
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo countAttribute;
            public static AttributeInfo materialAttribute;
            public static ChildInfo inputChild;
            public static ChildInfo pChild;
            public static ChildInfo extraChild;
        }

        public static class ListOfUInts
        {
            public static DomNodeType Type;
            public static AttributeInfo Attribute;
        }

        public static class polygons
        {
            public static DomNodeType Type;
            public static AttributeInfo pAttribute;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo countAttribute;
            public static AttributeInfo materialAttribute;
            public static ChildInfo inputChild;
            public static ChildInfo phChild;
            public static ChildInfo extraChild;
        }

        public static class polygons_ph
        {
            public static DomNodeType Type;
            public static AttributeInfo pAttribute;
            public static ChildInfo hChild;
        }

        public static class polylist
        {
            public static DomNodeType Type;
            public static AttributeInfo vcountAttribute;
            public static AttributeInfo pAttribute;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo countAttribute;
            public static AttributeInfo materialAttribute;
            public static ChildInfo inputChild;
            public static ChildInfo extraChild;
        }

        public static class triangles
        {
            public static DomNodeType Type;
            public static AttributeInfo pAttribute;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo countAttribute;
            public static AttributeInfo materialAttribute;
            public static ChildInfo inputChild;
            public static ChildInfo extraChild;
        }

        public static class trifans
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo countAttribute;
            public static AttributeInfo materialAttribute;
            public static ChildInfo inputChild;
            public static ChildInfo pChild;
            public static ChildInfo extraChild;
        }

        public static class tristrips
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo countAttribute;
            public static AttributeInfo materialAttribute;
            public static ChildInfo inputChild;
            public static ChildInfo pChild;
            public static ChildInfo extraChild;
        }

        public static class mesh
        {
            public static DomNodeType Type;
            public static ChildInfo sourceChild;
            public static ChildInfo verticesChild;
            public static ChildInfo linesChild;
            public static ChildInfo linestripsChild;
            public static ChildInfo polygonsChild;
            public static ChildInfo polylistChild;
            public static ChildInfo trianglesChild;
            public static ChildInfo trifansChild;
            public static ChildInfo tristripsChild;
            public static ChildInfo extraChild;
        }

        public static class spline
        {
            public static DomNodeType Type;
            public static AttributeInfo closedAttribute;
            public static ChildInfo sourceChild;
            public static ChildInfo control_verticesChild;
            public static ChildInfo extraChild;
        }

        public static class spline_control_vertices
        {
            public static DomNodeType Type;
            public static ChildInfo inputChild;
            public static ChildInfo extraChild;
        }

        public static class library_effects
        {
            public static DomNodeType Type;
            public static AttributeInfo idAttribute;
            public static AttributeInfo nameAttribute;
            public static ChildInfo assetChild;
            public static ChildInfo effectChild;
            public static ChildInfo extraChild;
        }

        public static class effect
        {
            public static DomNodeType Type;
            public static AttributeInfo idAttribute;
            public static AttributeInfo nameAttribute;
            public static ChildInfo assetChild;
            public static ChildInfo annotateChild;
            public static ChildInfo imageChild;
            public static ChildInfo newparamChild;
            public static ChildInfo fx_profile_abstractChild;
            public static ChildInfo profile_CGChild;
            public static ChildInfo profile_COMMONChild;
            public static ChildInfo extraChild;
        }

        public static class fx_annotate_common
        {
            public static DomNodeType Type;
            public static AttributeInfo boolAttribute;
            public static AttributeInfo bool2Attribute;
            public static AttributeInfo bool3Attribute;
            public static AttributeInfo bool4Attribute;
            public static AttributeInfo intAttribute;
            public static AttributeInfo int2Attribute;
            public static AttributeInfo int3Attribute;
            public static AttributeInfo int4Attribute;
            public static AttributeInfo floatAttribute;
            public static AttributeInfo float2Attribute;
            public static AttributeInfo float3Attribute;
            public static AttributeInfo float4Attribute;
            public static AttributeInfo float2x2Attribute;
            public static AttributeInfo float3x3Attribute;
            public static AttributeInfo float4x4Attribute;
            public static AttributeInfo stringAttribute;
            public static AttributeInfo nameAttribute;
        }

        public static class image
        {
            public static DomNodeType Type;
            public static AttributeInfo dataAttribute;
            public static AttributeInfo init_fromAttribute;
            public static AttributeInfo idAttribute;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo formatAttribute;
            public static AttributeInfo heightAttribute;
            public static AttributeInfo widthAttribute;
            public static AttributeInfo depthAttribute;
            public static ChildInfo assetChild;
            public static ChildInfo extraChild;
        }

        public static class fx_newparam_common
        {
            public static DomNodeType Type;
            public static AttributeInfo semanticAttribute;
            public static AttributeInfo modifierAttribute;
            public static AttributeInfo boolAttribute;
            public static AttributeInfo bool2Attribute;
            public static AttributeInfo bool3Attribute;
            public static AttributeInfo bool4Attribute;
            public static AttributeInfo intAttribute;
            public static AttributeInfo int2Attribute;
            public static AttributeInfo int3Attribute;
            public static AttributeInfo int4Attribute;
            public static AttributeInfo floatAttribute;
            public static AttributeInfo float2Attribute;
            public static AttributeInfo float3Attribute;
            public static AttributeInfo float4Attribute;
            public static AttributeInfo float1x1Attribute;
            public static AttributeInfo float1x2Attribute;
            public static AttributeInfo float1x3Attribute;
            public static AttributeInfo float1x4Attribute;
            public static AttributeInfo float2x1Attribute;
            public static AttributeInfo float2x2Attribute;
            public static AttributeInfo float2x3Attribute;
            public static AttributeInfo float2x4Attribute;
            public static AttributeInfo float3x1Attribute;
            public static AttributeInfo float3x2Attribute;
            public static AttributeInfo float3x3Attribute;
            public static AttributeInfo float3x4Attribute;
            public static AttributeInfo float4x1Attribute;
            public static AttributeInfo float4x2Attribute;
            public static AttributeInfo float4x3Attribute;
            public static AttributeInfo float4x4Attribute;
            public static AttributeInfo enumAttribute;
            public static AttributeInfo sidAttribute;
            public static ChildInfo annotateChild;
            public static ChildInfo surfaceChild;
            public static ChildInfo sampler1DChild;
            public static ChildInfo sampler2DChild;
            public static ChildInfo sampler3DChild;
            public static ChildInfo samplerCUBEChild;
            public static ChildInfo samplerRECTChild;
            public static ChildInfo samplerDEPTHChild;
        }

        public static class fx_surface_common
        {
            public static DomNodeType Type;
            public static AttributeInfo formatAttribute;
            public static AttributeInfo sizeAttribute;
            public static AttributeInfo viewport_ratioAttribute;
            public static AttributeInfo mip_levelsAttribute;
            public static AttributeInfo mipmap_generateAttribute;
            public static AttributeInfo typeAttribute;
            public static ChildInfo init_as_nullChild;
            public static ChildInfo init_as_targetChild;
            public static ChildInfo init_cubeChild;
            public static ChildInfo init_volumeChild;
            public static ChildInfo init_planarChild;
            public static ChildInfo init_fromChild;
            public static ChildInfo format_hintChild;
            public static ChildInfo extraChild;
        }

        public static class fx_surface_init_cube_common
        {
            public static DomNodeType Type;
            public static ChildInfo allChild;
            public static ChildInfo primaryChild;
            public static ChildInfo faceChild;
        }

        public static class fx_surface_init_cube_common_all
        {
            public static DomNodeType Type;
            public static AttributeInfo refAttribute;
        }

        public static class fx_surface_init_cube_common_primary
        {
            public static DomNodeType Type;
            public static AttributeInfo refAttribute;
            public static ChildInfo orderChild;
        }

        public static class fx_surface_face_enum
        {
            public static DomNodeType Type;
            public static AttributeInfo Attribute;
        }

        public static class fx_surface_init_cube_common_face
        {
            public static DomNodeType Type;
            public static AttributeInfo refAttribute;
        }

        public static class fx_surface_init_volume_common
        {
            public static DomNodeType Type;
            public static ChildInfo allChild;
            public static ChildInfo primaryChild;
        }

        public static class fx_surface_init_volume_common_all
        {
            public static DomNodeType Type;
            public static AttributeInfo refAttribute;
        }

        public static class fx_surface_init_volume_common_primary
        {
            public static DomNodeType Type;
            public static AttributeInfo refAttribute;
        }

        public static class fx_surface_init_planar_common
        {
            public static DomNodeType Type;
            public static ChildInfo allChild;
        }

        public static class fx_surface_init_planar_common_all
        {
            public static DomNodeType Type;
            public static AttributeInfo refAttribute;
        }

        public static class fx_surface_init_from_common
        {
            public static DomNodeType Type;
            public static AttributeInfo Attribute;
            public static AttributeInfo mipAttribute;
            public static AttributeInfo sliceAttribute;
            public static AttributeInfo faceAttribute;
        }

        public static class fx_surface_format_hint_common
        {
            public static DomNodeType Type;
            public static AttributeInfo channelsAttribute;
            public static AttributeInfo rangeAttribute;
            public static AttributeInfo precisionAttribute;
            public static ChildInfo optionChild;
            public static ChildInfo extraChild;
        }

        public static class fx_surface_format_hint_option_enum
        {
            public static DomNodeType Type;
            public static AttributeInfo Attribute;
        }

        public static class fx_sampler1D_common
        {
            public static DomNodeType Type;
            public static AttributeInfo sourceAttribute;
            public static AttributeInfo wrap_sAttribute;
            public static AttributeInfo minfilterAttribute;
            public static AttributeInfo magfilterAttribute;
            public static AttributeInfo mipfilterAttribute;
            public static AttributeInfo border_colorAttribute;
            public static AttributeInfo mipmap_maxlevelAttribute;
            public static AttributeInfo mipmap_biasAttribute;
            public static ChildInfo extraChild;
        }

        public static class fx_sampler2D_common
        {
            public static DomNodeType Type;
            public static AttributeInfo sourceAttribute;
            public static AttributeInfo wrap_sAttribute;
            public static AttributeInfo wrap_tAttribute;
            public static AttributeInfo minfilterAttribute;
            public static AttributeInfo magfilterAttribute;
            public static AttributeInfo mipfilterAttribute;
            public static AttributeInfo border_colorAttribute;
            public static AttributeInfo mipmap_maxlevelAttribute;
            public static AttributeInfo mipmap_biasAttribute;
            public static ChildInfo extraChild;
        }

        public static class fx_sampler3D_common
        {
            public static DomNodeType Type;
            public static AttributeInfo sourceAttribute;
            public static AttributeInfo wrap_sAttribute;
            public static AttributeInfo wrap_tAttribute;
            public static AttributeInfo wrap_pAttribute;
            public static AttributeInfo minfilterAttribute;
            public static AttributeInfo magfilterAttribute;
            public static AttributeInfo mipfilterAttribute;
            public static AttributeInfo border_colorAttribute;
            public static AttributeInfo mipmap_maxlevelAttribute;
            public static AttributeInfo mipmap_biasAttribute;
            public static ChildInfo extraChild;
        }

        public static class fx_samplerCUBE_common
        {
            public static DomNodeType Type;
            public static AttributeInfo sourceAttribute;
            public static AttributeInfo wrap_sAttribute;
            public static AttributeInfo wrap_tAttribute;
            public static AttributeInfo wrap_pAttribute;
            public static AttributeInfo minfilterAttribute;
            public static AttributeInfo magfilterAttribute;
            public static AttributeInfo mipfilterAttribute;
            public static AttributeInfo border_colorAttribute;
            public static AttributeInfo mipmap_maxlevelAttribute;
            public static AttributeInfo mipmap_biasAttribute;
            public static ChildInfo extraChild;
        }

        public static class fx_samplerRECT_common
        {
            public static DomNodeType Type;
            public static AttributeInfo sourceAttribute;
            public static AttributeInfo wrap_sAttribute;
            public static AttributeInfo wrap_tAttribute;
            public static AttributeInfo minfilterAttribute;
            public static AttributeInfo magfilterAttribute;
            public static AttributeInfo mipfilterAttribute;
            public static AttributeInfo border_colorAttribute;
            public static AttributeInfo mipmap_maxlevelAttribute;
            public static AttributeInfo mipmap_biasAttribute;
            public static ChildInfo extraChild;
        }

        public static class fx_samplerDEPTH_common
        {
            public static DomNodeType Type;
            public static AttributeInfo sourceAttribute;
            public static AttributeInfo wrap_sAttribute;
            public static AttributeInfo wrap_tAttribute;
            public static AttributeInfo minfilterAttribute;
            public static AttributeInfo magfilterAttribute;
            public static ChildInfo extraChild;
        }

        public static class profile_CG
        {
            public static DomNodeType Type;
            public static AttributeInfo idAttribute;
            public static AttributeInfo platformAttribute;
            public static ChildInfo assetChild;
            public static ChildInfo codeChild;
            public static ChildInfo includeChild;
            public static ChildInfo imageChild;
            public static ChildInfo newparamChild;
            public static ChildInfo techniqueChild;
            public static ChildInfo extraChild;
        }

        public static class fx_code_profile
        {
            public static DomNodeType Type;
            public static AttributeInfo Attribute;
            public static AttributeInfo sidAttribute;
        }

        public static class fx_include_common
        {
            public static DomNodeType Type;
            public static AttributeInfo sidAttribute;
            public static AttributeInfo urlAttribute;
        }

        public static class cg_newparam
        {
            public static DomNodeType Type;
            public static AttributeInfo semanticAttribute;
            public static AttributeInfo modifierAttribute;
            public static AttributeInfo boolAttribute;
            public static AttributeInfo bool1Attribute;
            public static AttributeInfo bool2Attribute;
            public static AttributeInfo bool3Attribute;
            public static AttributeInfo bool4Attribute;
            public static AttributeInfo bool1x1Attribute;
            public static AttributeInfo bool1x2Attribute;
            public static AttributeInfo bool1x3Attribute;
            public static AttributeInfo bool1x4Attribute;
            public static AttributeInfo bool2x1Attribute;
            public static AttributeInfo bool2x2Attribute;
            public static AttributeInfo bool2x3Attribute;
            public static AttributeInfo bool2x4Attribute;
            public static AttributeInfo bool3x1Attribute;
            public static AttributeInfo bool3x2Attribute;
            public static AttributeInfo bool3x3Attribute;
            public static AttributeInfo bool3x4Attribute;
            public static AttributeInfo bool4x1Attribute;
            public static AttributeInfo bool4x2Attribute;
            public static AttributeInfo bool4x3Attribute;
            public static AttributeInfo bool4x4Attribute;
            public static AttributeInfo floatAttribute;
            public static AttributeInfo float1Attribute;
            public static AttributeInfo float2Attribute;
            public static AttributeInfo float3Attribute;
            public static AttributeInfo float4Attribute;
            public static AttributeInfo float1x1Attribute;
            public static AttributeInfo float1x2Attribute;
            public static AttributeInfo float1x3Attribute;
            public static AttributeInfo float1x4Attribute;
            public static AttributeInfo float2x1Attribute;
            public static AttributeInfo float2x2Attribute;
            public static AttributeInfo float2x3Attribute;
            public static AttributeInfo float2x4Attribute;
            public static AttributeInfo float3x1Attribute;
            public static AttributeInfo float3x2Attribute;
            public static AttributeInfo float3x3Attribute;
            public static AttributeInfo float3x4Attribute;
            public static AttributeInfo float4x1Attribute;
            public static AttributeInfo float4x2Attribute;
            public static AttributeInfo float4x3Attribute;
            public static AttributeInfo float4x4Attribute;
            public static AttributeInfo intAttribute;
            public static AttributeInfo int1Attribute;
            public static AttributeInfo int2Attribute;
            public static AttributeInfo int3Attribute;
            public static AttributeInfo int4Attribute;
            public static AttributeInfo int1x1Attribute;
            public static AttributeInfo int1x2Attribute;
            public static AttributeInfo int1x3Attribute;
            public static AttributeInfo int1x4Attribute;
            public static AttributeInfo int2x1Attribute;
            public static AttributeInfo int2x2Attribute;
            public static AttributeInfo int2x3Attribute;
            public static AttributeInfo int2x4Attribute;
            public static AttributeInfo int3x1Attribute;
            public static AttributeInfo int3x2Attribute;
            public static AttributeInfo int3x3Attribute;
            public static AttributeInfo int3x4Attribute;
            public static AttributeInfo int4x1Attribute;
            public static AttributeInfo int4x2Attribute;
            public static AttributeInfo int4x3Attribute;
            public static AttributeInfo int4x4Attribute;
            public static AttributeInfo halfAttribute;
            public static AttributeInfo half1Attribute;
            public static AttributeInfo half2Attribute;
            public static AttributeInfo half3Attribute;
            public static AttributeInfo half4Attribute;
            public static AttributeInfo half1x1Attribute;
            public static AttributeInfo half1x2Attribute;
            public static AttributeInfo half1x3Attribute;
            public static AttributeInfo half1x4Attribute;
            public static AttributeInfo half2x1Attribute;
            public static AttributeInfo half2x2Attribute;
            public static AttributeInfo half2x3Attribute;
            public static AttributeInfo half2x4Attribute;
            public static AttributeInfo half3x1Attribute;
            public static AttributeInfo half3x2Attribute;
            public static AttributeInfo half3x3Attribute;
            public static AttributeInfo half3x4Attribute;
            public static AttributeInfo half4x1Attribute;
            public static AttributeInfo half4x2Attribute;
            public static AttributeInfo half4x3Attribute;
            public static AttributeInfo half4x4Attribute;
            public static AttributeInfo fixedAttribute;
            public static AttributeInfo fixed1Attribute;
            public static AttributeInfo fixed2Attribute;
            public static AttributeInfo fixed3Attribute;
            public static AttributeInfo fixed4Attribute;
            public static AttributeInfo fixed1x1Attribute;
            public static AttributeInfo fixed1x2Attribute;
            public static AttributeInfo fixed1x3Attribute;
            public static AttributeInfo fixed1x4Attribute;
            public static AttributeInfo fixed2x1Attribute;
            public static AttributeInfo fixed2x2Attribute;
            public static AttributeInfo fixed2x3Attribute;
            public static AttributeInfo fixed2x4Attribute;
            public static AttributeInfo fixed3x1Attribute;
            public static AttributeInfo fixed3x2Attribute;
            public static AttributeInfo fixed3x3Attribute;
            public static AttributeInfo fixed3x4Attribute;
            public static AttributeInfo fixed4x1Attribute;
            public static AttributeInfo fixed4x2Attribute;
            public static AttributeInfo fixed4x3Attribute;
            public static AttributeInfo fixed4x4Attribute;
            public static AttributeInfo stringAttribute;
            public static AttributeInfo enumAttribute;
            public static AttributeInfo sidAttribute;
            public static ChildInfo annotateChild;
            public static ChildInfo surfaceChild;
            public static ChildInfo sampler1DChild;
            public static ChildInfo sampler2DChild;
            public static ChildInfo sampler3DChild;
            public static ChildInfo samplerRECTChild;
            public static ChildInfo samplerCUBEChild;
            public static ChildInfo samplerDEPTHChild;
            public static ChildInfo usertypeChild;
            public static ChildInfo arrayChild;
        }

        public static class cg_surface_type
        {
            public static DomNodeType Type;
            public static AttributeInfo formatAttribute;
            public static AttributeInfo sizeAttribute;
            public static AttributeInfo viewport_ratioAttribute;
            public static AttributeInfo mip_levelsAttribute;
            public static AttributeInfo mipmap_generateAttribute;
            public static AttributeInfo typeAttribute;
            public static ChildInfo init_as_nullChild;
            public static ChildInfo init_as_targetChild;
            public static ChildInfo init_cubeChild;
            public static ChildInfo init_volumeChild;
            public static ChildInfo init_planarChild;
            public static ChildInfo init_fromChild;
            public static ChildInfo format_hintChild;
            public static ChildInfo extraChild;
            public static ChildInfo generatorChild;
        }

        public static class cg_surface_type_generator
        {
            public static DomNodeType Type;
            public static ChildInfo annotateChild;
            public static ChildInfo codeChild;
            public static ChildInfo includeChild;
            public static ChildInfo nameChild;
            public static ChildInfo setparamChild;
        }

        public static class generator_name
        {
            public static DomNodeType Type;
            public static AttributeInfo Attribute;
            public static AttributeInfo sourceAttribute;
        }

        public static class cg_setparam_simple
        {
            public static DomNodeType Type;
            public static AttributeInfo boolAttribute;
            public static AttributeInfo bool1Attribute;
            public static AttributeInfo bool2Attribute;
            public static AttributeInfo bool3Attribute;
            public static AttributeInfo bool4Attribute;
            public static AttributeInfo bool1x1Attribute;
            public static AttributeInfo bool1x2Attribute;
            public static AttributeInfo bool1x3Attribute;
            public static AttributeInfo bool1x4Attribute;
            public static AttributeInfo bool2x1Attribute;
            public static AttributeInfo bool2x2Attribute;
            public static AttributeInfo bool2x3Attribute;
            public static AttributeInfo bool2x4Attribute;
            public static AttributeInfo bool3x1Attribute;
            public static AttributeInfo bool3x2Attribute;
            public static AttributeInfo bool3x3Attribute;
            public static AttributeInfo bool3x4Attribute;
            public static AttributeInfo bool4x1Attribute;
            public static AttributeInfo bool4x2Attribute;
            public static AttributeInfo bool4x3Attribute;
            public static AttributeInfo bool4x4Attribute;
            public static AttributeInfo floatAttribute;
            public static AttributeInfo float1Attribute;
            public static AttributeInfo float2Attribute;
            public static AttributeInfo float3Attribute;
            public static AttributeInfo float4Attribute;
            public static AttributeInfo float1x1Attribute;
            public static AttributeInfo float1x2Attribute;
            public static AttributeInfo float1x3Attribute;
            public static AttributeInfo float1x4Attribute;
            public static AttributeInfo float2x1Attribute;
            public static AttributeInfo float2x2Attribute;
            public static AttributeInfo float2x3Attribute;
            public static AttributeInfo float2x4Attribute;
            public static AttributeInfo float3x1Attribute;
            public static AttributeInfo float3x2Attribute;
            public static AttributeInfo float3x3Attribute;
            public static AttributeInfo float3x4Attribute;
            public static AttributeInfo float4x1Attribute;
            public static AttributeInfo float4x2Attribute;
            public static AttributeInfo float4x3Attribute;
            public static AttributeInfo float4x4Attribute;
            public static AttributeInfo intAttribute;
            public static AttributeInfo int1Attribute;
            public static AttributeInfo int2Attribute;
            public static AttributeInfo int3Attribute;
            public static AttributeInfo int4Attribute;
            public static AttributeInfo int1x1Attribute;
            public static AttributeInfo int1x2Attribute;
            public static AttributeInfo int1x3Attribute;
            public static AttributeInfo int1x4Attribute;
            public static AttributeInfo int2x1Attribute;
            public static AttributeInfo int2x2Attribute;
            public static AttributeInfo int2x3Attribute;
            public static AttributeInfo int2x4Attribute;
            public static AttributeInfo int3x1Attribute;
            public static AttributeInfo int3x2Attribute;
            public static AttributeInfo int3x3Attribute;
            public static AttributeInfo int3x4Attribute;
            public static AttributeInfo int4x1Attribute;
            public static AttributeInfo int4x2Attribute;
            public static AttributeInfo int4x3Attribute;
            public static AttributeInfo int4x4Attribute;
            public static AttributeInfo halfAttribute;
            public static AttributeInfo half1Attribute;
            public static AttributeInfo half2Attribute;
            public static AttributeInfo half3Attribute;
            public static AttributeInfo half4Attribute;
            public static AttributeInfo half1x1Attribute;
            public static AttributeInfo half1x2Attribute;
            public static AttributeInfo half1x3Attribute;
            public static AttributeInfo half1x4Attribute;
            public static AttributeInfo half2x1Attribute;
            public static AttributeInfo half2x2Attribute;
            public static AttributeInfo half2x3Attribute;
            public static AttributeInfo half2x4Attribute;
            public static AttributeInfo half3x1Attribute;
            public static AttributeInfo half3x2Attribute;
            public static AttributeInfo half3x3Attribute;
            public static AttributeInfo half3x4Attribute;
            public static AttributeInfo half4x1Attribute;
            public static AttributeInfo half4x2Attribute;
            public static AttributeInfo half4x3Attribute;
            public static AttributeInfo half4x4Attribute;
            public static AttributeInfo fixedAttribute;
            public static AttributeInfo fixed1Attribute;
            public static AttributeInfo fixed2Attribute;
            public static AttributeInfo fixed3Attribute;
            public static AttributeInfo fixed4Attribute;
            public static AttributeInfo fixed1x1Attribute;
            public static AttributeInfo fixed1x2Attribute;
            public static AttributeInfo fixed1x3Attribute;
            public static AttributeInfo fixed1x4Attribute;
            public static AttributeInfo fixed2x1Attribute;
            public static AttributeInfo fixed2x2Attribute;
            public static AttributeInfo fixed2x3Attribute;
            public static AttributeInfo fixed2x4Attribute;
            public static AttributeInfo fixed3x1Attribute;
            public static AttributeInfo fixed3x2Attribute;
            public static AttributeInfo fixed3x3Attribute;
            public static AttributeInfo fixed3x4Attribute;
            public static AttributeInfo fixed4x1Attribute;
            public static AttributeInfo fixed4x2Attribute;
            public static AttributeInfo fixed4x3Attribute;
            public static AttributeInfo fixed4x4Attribute;
            public static AttributeInfo stringAttribute;
            public static AttributeInfo enumAttribute;
            public static AttributeInfo refAttribute;
            public static ChildInfo annotateChild;
            public static ChildInfo surfaceChild;
            public static ChildInfo sampler1DChild;
            public static ChildInfo sampler2DChild;
            public static ChildInfo sampler3DChild;
            public static ChildInfo samplerRECTChild;
            public static ChildInfo samplerCUBEChild;
            public static ChildInfo samplerDEPTHChild;
        }

        public static class cg_sampler1D
        {
            public static DomNodeType Type;
            public static AttributeInfo sourceAttribute;
            public static AttributeInfo wrap_sAttribute;
            public static AttributeInfo minfilterAttribute;
            public static AttributeInfo magfilterAttribute;
            public static AttributeInfo mipfilterAttribute;
            public static AttributeInfo border_colorAttribute;
            public static AttributeInfo mipmap_maxlevelAttribute;
            public static AttributeInfo mipmap_biasAttribute;
            public static ChildInfo extraChild;
        }

        public static class cg_sampler2D
        {
            public static DomNodeType Type;
            public static AttributeInfo sourceAttribute;
            public static AttributeInfo wrap_sAttribute;
            public static AttributeInfo wrap_tAttribute;
            public static AttributeInfo minfilterAttribute;
            public static AttributeInfo magfilterAttribute;
            public static AttributeInfo mipfilterAttribute;
            public static AttributeInfo border_colorAttribute;
            public static AttributeInfo mipmap_maxlevelAttribute;
            public static AttributeInfo mipmap_biasAttribute;
            public static ChildInfo extraChild;
        }

        public static class cg_sampler3D
        {
            public static DomNodeType Type;
            public static AttributeInfo sourceAttribute;
            public static AttributeInfo wrap_sAttribute;
            public static AttributeInfo wrap_tAttribute;
            public static AttributeInfo wrap_pAttribute;
            public static AttributeInfo minfilterAttribute;
            public static AttributeInfo magfilterAttribute;
            public static AttributeInfo mipfilterAttribute;
            public static AttributeInfo border_colorAttribute;
            public static AttributeInfo mipmap_maxlevelAttribute;
            public static AttributeInfo mipmap_biasAttribute;
            public static ChildInfo extraChild;
        }

        public static class cg_samplerRECT
        {
            public static DomNodeType Type;
            public static AttributeInfo sourceAttribute;
            public static AttributeInfo wrap_sAttribute;
            public static AttributeInfo wrap_tAttribute;
            public static AttributeInfo minfilterAttribute;
            public static AttributeInfo magfilterAttribute;
            public static AttributeInfo mipfilterAttribute;
            public static AttributeInfo border_colorAttribute;
            public static AttributeInfo mipmap_maxlevelAttribute;
            public static AttributeInfo mipmap_biasAttribute;
            public static ChildInfo extraChild;
        }

        public static class cg_samplerCUBE
        {
            public static DomNodeType Type;
            public static AttributeInfo sourceAttribute;
            public static AttributeInfo wrap_sAttribute;
            public static AttributeInfo wrap_tAttribute;
            public static AttributeInfo wrap_pAttribute;
            public static AttributeInfo minfilterAttribute;
            public static AttributeInfo magfilterAttribute;
            public static AttributeInfo mipfilterAttribute;
            public static AttributeInfo border_colorAttribute;
            public static AttributeInfo mipmap_maxlevelAttribute;
            public static AttributeInfo mipmap_biasAttribute;
            public static ChildInfo extraChild;
        }

        public static class cg_samplerDEPTH
        {
            public static DomNodeType Type;
            public static AttributeInfo sourceAttribute;
            public static AttributeInfo wrap_sAttribute;
            public static AttributeInfo wrap_tAttribute;
            public static AttributeInfo minfilterAttribute;
            public static AttributeInfo magfilterAttribute;
            public static ChildInfo extraChild;
        }

        public static class cg_setuser_type
        {
            public static DomNodeType Type;
            public static AttributeInfo boolAttribute;
            public static AttributeInfo bool1Attribute;
            public static AttributeInfo bool2Attribute;
            public static AttributeInfo bool3Attribute;
            public static AttributeInfo bool4Attribute;
            public static AttributeInfo bool1x1Attribute;
            public static AttributeInfo bool1x2Attribute;
            public static AttributeInfo bool1x3Attribute;
            public static AttributeInfo bool1x4Attribute;
            public static AttributeInfo bool2x1Attribute;
            public static AttributeInfo bool2x2Attribute;
            public static AttributeInfo bool2x3Attribute;
            public static AttributeInfo bool2x4Attribute;
            public static AttributeInfo bool3x1Attribute;
            public static AttributeInfo bool3x2Attribute;
            public static AttributeInfo bool3x3Attribute;
            public static AttributeInfo bool3x4Attribute;
            public static AttributeInfo bool4x1Attribute;
            public static AttributeInfo bool4x2Attribute;
            public static AttributeInfo bool4x3Attribute;
            public static AttributeInfo bool4x4Attribute;
            public static AttributeInfo floatAttribute;
            public static AttributeInfo float1Attribute;
            public static AttributeInfo float2Attribute;
            public static AttributeInfo float3Attribute;
            public static AttributeInfo float4Attribute;
            public static AttributeInfo float1x1Attribute;
            public static AttributeInfo float1x2Attribute;
            public static AttributeInfo float1x3Attribute;
            public static AttributeInfo float1x4Attribute;
            public static AttributeInfo float2x1Attribute;
            public static AttributeInfo float2x2Attribute;
            public static AttributeInfo float2x3Attribute;
            public static AttributeInfo float2x4Attribute;
            public static AttributeInfo float3x1Attribute;
            public static AttributeInfo float3x2Attribute;
            public static AttributeInfo float3x3Attribute;
            public static AttributeInfo float3x4Attribute;
            public static AttributeInfo float4x1Attribute;
            public static AttributeInfo float4x2Attribute;
            public static AttributeInfo float4x3Attribute;
            public static AttributeInfo float4x4Attribute;
            public static AttributeInfo intAttribute;
            public static AttributeInfo int1Attribute;
            public static AttributeInfo int2Attribute;
            public static AttributeInfo int3Attribute;
            public static AttributeInfo int4Attribute;
            public static AttributeInfo int1x1Attribute;
            public static AttributeInfo int1x2Attribute;
            public static AttributeInfo int1x3Attribute;
            public static AttributeInfo int1x4Attribute;
            public static AttributeInfo int2x1Attribute;
            public static AttributeInfo int2x2Attribute;
            public static AttributeInfo int2x3Attribute;
            public static AttributeInfo int2x4Attribute;
            public static AttributeInfo int3x1Attribute;
            public static AttributeInfo int3x2Attribute;
            public static AttributeInfo int3x3Attribute;
            public static AttributeInfo int3x4Attribute;
            public static AttributeInfo int4x1Attribute;
            public static AttributeInfo int4x2Attribute;
            public static AttributeInfo int4x3Attribute;
            public static AttributeInfo int4x4Attribute;
            public static AttributeInfo halfAttribute;
            public static AttributeInfo half1Attribute;
            public static AttributeInfo half2Attribute;
            public static AttributeInfo half3Attribute;
            public static AttributeInfo half4Attribute;
            public static AttributeInfo half1x1Attribute;
            public static AttributeInfo half1x2Attribute;
            public static AttributeInfo half1x3Attribute;
            public static AttributeInfo half1x4Attribute;
            public static AttributeInfo half2x1Attribute;
            public static AttributeInfo half2x2Attribute;
            public static AttributeInfo half2x3Attribute;
            public static AttributeInfo half2x4Attribute;
            public static AttributeInfo half3x1Attribute;
            public static AttributeInfo half3x2Attribute;
            public static AttributeInfo half3x3Attribute;
            public static AttributeInfo half3x4Attribute;
            public static AttributeInfo half4x1Attribute;
            public static AttributeInfo half4x2Attribute;
            public static AttributeInfo half4x3Attribute;
            public static AttributeInfo half4x4Attribute;
            public static AttributeInfo fixedAttribute;
            public static AttributeInfo fixed1Attribute;
            public static AttributeInfo fixed2Attribute;
            public static AttributeInfo fixed3Attribute;
            public static AttributeInfo fixed4Attribute;
            public static AttributeInfo fixed1x1Attribute;
            public static AttributeInfo fixed1x2Attribute;
            public static AttributeInfo fixed1x3Attribute;
            public static AttributeInfo fixed1x4Attribute;
            public static AttributeInfo fixed2x1Attribute;
            public static AttributeInfo fixed2x2Attribute;
            public static AttributeInfo fixed2x3Attribute;
            public static AttributeInfo fixed2x4Attribute;
            public static AttributeInfo fixed3x1Attribute;
            public static AttributeInfo fixed3x2Attribute;
            public static AttributeInfo fixed3x3Attribute;
            public static AttributeInfo fixed3x4Attribute;
            public static AttributeInfo fixed4x1Attribute;
            public static AttributeInfo fixed4x2Attribute;
            public static AttributeInfo fixed4x3Attribute;
            public static AttributeInfo fixed4x4Attribute;
            public static AttributeInfo stringAttribute;
            public static AttributeInfo enumAttribute;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo sourceAttribute;
            public static ChildInfo surfaceChild;
            public static ChildInfo sampler1DChild;
            public static ChildInfo sampler2DChild;
            public static ChildInfo sampler3DChild;
            public static ChildInfo samplerRECTChild;
            public static ChildInfo samplerCUBEChild;
            public static ChildInfo samplerDEPTHChild;
            public static ChildInfo arrayChild;
            public static ChildInfo usertypeChild;
            public static ChildInfo connect_paramChild;
            public static ChildInfo setparamChild;
        }

        public static class cg_setarray_type
        {
            public static DomNodeType Type;
            public static AttributeInfo boolAttribute;
            public static AttributeInfo bool1Attribute;
            public static AttributeInfo bool2Attribute;
            public static AttributeInfo bool3Attribute;
            public static AttributeInfo bool4Attribute;
            public static AttributeInfo bool1x1Attribute;
            public static AttributeInfo bool1x2Attribute;
            public static AttributeInfo bool1x3Attribute;
            public static AttributeInfo bool1x4Attribute;
            public static AttributeInfo bool2x1Attribute;
            public static AttributeInfo bool2x2Attribute;
            public static AttributeInfo bool2x3Attribute;
            public static AttributeInfo bool2x4Attribute;
            public static AttributeInfo bool3x1Attribute;
            public static AttributeInfo bool3x2Attribute;
            public static AttributeInfo bool3x3Attribute;
            public static AttributeInfo bool3x4Attribute;
            public static AttributeInfo bool4x1Attribute;
            public static AttributeInfo bool4x2Attribute;
            public static AttributeInfo bool4x3Attribute;
            public static AttributeInfo bool4x4Attribute;
            public static AttributeInfo floatAttribute;
            public static AttributeInfo float1Attribute;
            public static AttributeInfo float2Attribute;
            public static AttributeInfo float3Attribute;
            public static AttributeInfo float4Attribute;
            public static AttributeInfo float1x1Attribute;
            public static AttributeInfo float1x2Attribute;
            public static AttributeInfo float1x3Attribute;
            public static AttributeInfo float1x4Attribute;
            public static AttributeInfo float2x1Attribute;
            public static AttributeInfo float2x2Attribute;
            public static AttributeInfo float2x3Attribute;
            public static AttributeInfo float2x4Attribute;
            public static AttributeInfo float3x1Attribute;
            public static AttributeInfo float3x2Attribute;
            public static AttributeInfo float3x3Attribute;
            public static AttributeInfo float3x4Attribute;
            public static AttributeInfo float4x1Attribute;
            public static AttributeInfo float4x2Attribute;
            public static AttributeInfo float4x3Attribute;
            public static AttributeInfo float4x4Attribute;
            public static AttributeInfo intAttribute;
            public static AttributeInfo int1Attribute;
            public static AttributeInfo int2Attribute;
            public static AttributeInfo int3Attribute;
            public static AttributeInfo int4Attribute;
            public static AttributeInfo int1x1Attribute;
            public static AttributeInfo int1x2Attribute;
            public static AttributeInfo int1x3Attribute;
            public static AttributeInfo int1x4Attribute;
            public static AttributeInfo int2x1Attribute;
            public static AttributeInfo int2x2Attribute;
            public static AttributeInfo int2x3Attribute;
            public static AttributeInfo int2x4Attribute;
            public static AttributeInfo int3x1Attribute;
            public static AttributeInfo int3x2Attribute;
            public static AttributeInfo int3x3Attribute;
            public static AttributeInfo int3x4Attribute;
            public static AttributeInfo int4x1Attribute;
            public static AttributeInfo int4x2Attribute;
            public static AttributeInfo int4x3Attribute;
            public static AttributeInfo int4x4Attribute;
            public static AttributeInfo halfAttribute;
            public static AttributeInfo half1Attribute;
            public static AttributeInfo half2Attribute;
            public static AttributeInfo half3Attribute;
            public static AttributeInfo half4Attribute;
            public static AttributeInfo half1x1Attribute;
            public static AttributeInfo half1x2Attribute;
            public static AttributeInfo half1x3Attribute;
            public static AttributeInfo half1x4Attribute;
            public static AttributeInfo half2x1Attribute;
            public static AttributeInfo half2x2Attribute;
            public static AttributeInfo half2x3Attribute;
            public static AttributeInfo half2x4Attribute;
            public static AttributeInfo half3x1Attribute;
            public static AttributeInfo half3x2Attribute;
            public static AttributeInfo half3x3Attribute;
            public static AttributeInfo half3x4Attribute;
            public static AttributeInfo half4x1Attribute;
            public static AttributeInfo half4x2Attribute;
            public static AttributeInfo half4x3Attribute;
            public static AttributeInfo half4x4Attribute;
            public static AttributeInfo fixedAttribute;
            public static AttributeInfo fixed1Attribute;
            public static AttributeInfo fixed2Attribute;
            public static AttributeInfo fixed3Attribute;
            public static AttributeInfo fixed4Attribute;
            public static AttributeInfo fixed1x1Attribute;
            public static AttributeInfo fixed1x2Attribute;
            public static AttributeInfo fixed1x3Attribute;
            public static AttributeInfo fixed1x4Attribute;
            public static AttributeInfo fixed2x1Attribute;
            public static AttributeInfo fixed2x2Attribute;
            public static AttributeInfo fixed2x3Attribute;
            public static AttributeInfo fixed2x4Attribute;
            public static AttributeInfo fixed3x1Attribute;
            public static AttributeInfo fixed3x2Attribute;
            public static AttributeInfo fixed3x3Attribute;
            public static AttributeInfo fixed3x4Attribute;
            public static AttributeInfo fixed4x1Attribute;
            public static AttributeInfo fixed4x2Attribute;
            public static AttributeInfo fixed4x3Attribute;
            public static AttributeInfo fixed4x4Attribute;
            public static AttributeInfo stringAttribute;
            public static AttributeInfo enumAttribute;
            public static AttributeInfo lengthAttribute;
            public static ChildInfo surfaceChild;
            public static ChildInfo sampler1DChild;
            public static ChildInfo sampler2DChild;
            public static ChildInfo sampler3DChild;
            public static ChildInfo samplerRECTChild;
            public static ChildInfo samplerCUBEChild;
            public static ChildInfo samplerDEPTHChild;
            public static ChildInfo arrayChild;
            public static ChildInfo usertypeChild;
        }

        public static class cg_connect_param
        {
            public static DomNodeType Type;
            public static AttributeInfo refAttribute;
        }

        public static class cg_setparam
        {
            public static DomNodeType Type;
            public static AttributeInfo boolAttribute;
            public static AttributeInfo bool1Attribute;
            public static AttributeInfo bool2Attribute;
            public static AttributeInfo bool3Attribute;
            public static AttributeInfo bool4Attribute;
            public static AttributeInfo bool1x1Attribute;
            public static AttributeInfo bool1x2Attribute;
            public static AttributeInfo bool1x3Attribute;
            public static AttributeInfo bool1x4Attribute;
            public static AttributeInfo bool2x1Attribute;
            public static AttributeInfo bool2x2Attribute;
            public static AttributeInfo bool2x3Attribute;
            public static AttributeInfo bool2x4Attribute;
            public static AttributeInfo bool3x1Attribute;
            public static AttributeInfo bool3x2Attribute;
            public static AttributeInfo bool3x3Attribute;
            public static AttributeInfo bool3x4Attribute;
            public static AttributeInfo bool4x1Attribute;
            public static AttributeInfo bool4x2Attribute;
            public static AttributeInfo bool4x3Attribute;
            public static AttributeInfo bool4x4Attribute;
            public static AttributeInfo floatAttribute;
            public static AttributeInfo float1Attribute;
            public static AttributeInfo float2Attribute;
            public static AttributeInfo float3Attribute;
            public static AttributeInfo float4Attribute;
            public static AttributeInfo float1x1Attribute;
            public static AttributeInfo float1x2Attribute;
            public static AttributeInfo float1x3Attribute;
            public static AttributeInfo float1x4Attribute;
            public static AttributeInfo float2x1Attribute;
            public static AttributeInfo float2x2Attribute;
            public static AttributeInfo float2x3Attribute;
            public static AttributeInfo float2x4Attribute;
            public static AttributeInfo float3x1Attribute;
            public static AttributeInfo float3x2Attribute;
            public static AttributeInfo float3x3Attribute;
            public static AttributeInfo float3x4Attribute;
            public static AttributeInfo float4x1Attribute;
            public static AttributeInfo float4x2Attribute;
            public static AttributeInfo float4x3Attribute;
            public static AttributeInfo float4x4Attribute;
            public static AttributeInfo intAttribute;
            public static AttributeInfo int1Attribute;
            public static AttributeInfo int2Attribute;
            public static AttributeInfo int3Attribute;
            public static AttributeInfo int4Attribute;
            public static AttributeInfo int1x1Attribute;
            public static AttributeInfo int1x2Attribute;
            public static AttributeInfo int1x3Attribute;
            public static AttributeInfo int1x4Attribute;
            public static AttributeInfo int2x1Attribute;
            public static AttributeInfo int2x2Attribute;
            public static AttributeInfo int2x3Attribute;
            public static AttributeInfo int2x4Attribute;
            public static AttributeInfo int3x1Attribute;
            public static AttributeInfo int3x2Attribute;
            public static AttributeInfo int3x3Attribute;
            public static AttributeInfo int3x4Attribute;
            public static AttributeInfo int4x1Attribute;
            public static AttributeInfo int4x2Attribute;
            public static AttributeInfo int4x3Attribute;
            public static AttributeInfo int4x4Attribute;
            public static AttributeInfo halfAttribute;
            public static AttributeInfo half1Attribute;
            public static AttributeInfo half2Attribute;
            public static AttributeInfo half3Attribute;
            public static AttributeInfo half4Attribute;
            public static AttributeInfo half1x1Attribute;
            public static AttributeInfo half1x2Attribute;
            public static AttributeInfo half1x3Attribute;
            public static AttributeInfo half1x4Attribute;
            public static AttributeInfo half2x1Attribute;
            public static AttributeInfo half2x2Attribute;
            public static AttributeInfo half2x3Attribute;
            public static AttributeInfo half2x4Attribute;
            public static AttributeInfo half3x1Attribute;
            public static AttributeInfo half3x2Attribute;
            public static AttributeInfo half3x3Attribute;
            public static AttributeInfo half3x4Attribute;
            public static AttributeInfo half4x1Attribute;
            public static AttributeInfo half4x2Attribute;
            public static AttributeInfo half4x3Attribute;
            public static AttributeInfo half4x4Attribute;
            public static AttributeInfo fixedAttribute;
            public static AttributeInfo fixed1Attribute;
            public static AttributeInfo fixed2Attribute;
            public static AttributeInfo fixed3Attribute;
            public static AttributeInfo fixed4Attribute;
            public static AttributeInfo fixed1x1Attribute;
            public static AttributeInfo fixed1x2Attribute;
            public static AttributeInfo fixed1x3Attribute;
            public static AttributeInfo fixed1x4Attribute;
            public static AttributeInfo fixed2x1Attribute;
            public static AttributeInfo fixed2x2Attribute;
            public static AttributeInfo fixed2x3Attribute;
            public static AttributeInfo fixed2x4Attribute;
            public static AttributeInfo fixed3x1Attribute;
            public static AttributeInfo fixed3x2Attribute;
            public static AttributeInfo fixed3x3Attribute;
            public static AttributeInfo fixed3x4Attribute;
            public static AttributeInfo fixed4x1Attribute;
            public static AttributeInfo fixed4x2Attribute;
            public static AttributeInfo fixed4x3Attribute;
            public static AttributeInfo fixed4x4Attribute;
            public static AttributeInfo stringAttribute;
            public static AttributeInfo enumAttribute;
            public static AttributeInfo refAttribute;
            public static AttributeInfo programAttribute;
            public static ChildInfo surfaceChild;
            public static ChildInfo sampler1DChild;
            public static ChildInfo sampler2DChild;
            public static ChildInfo sampler3DChild;
            public static ChildInfo samplerRECTChild;
            public static ChildInfo samplerCUBEChild;
            public static ChildInfo samplerDEPTHChild;
            public static ChildInfo usertypeChild;
            public static ChildInfo arrayChild;
            public static ChildInfo connect_paramChild;
        }

        public static class cg_newarray_type
        {
            public static DomNodeType Type;
            public static AttributeInfo boolAttribute;
            public static AttributeInfo bool1Attribute;
            public static AttributeInfo bool2Attribute;
            public static AttributeInfo bool3Attribute;
            public static AttributeInfo bool4Attribute;
            public static AttributeInfo bool1x1Attribute;
            public static AttributeInfo bool1x2Attribute;
            public static AttributeInfo bool1x3Attribute;
            public static AttributeInfo bool1x4Attribute;
            public static AttributeInfo bool2x1Attribute;
            public static AttributeInfo bool2x2Attribute;
            public static AttributeInfo bool2x3Attribute;
            public static AttributeInfo bool2x4Attribute;
            public static AttributeInfo bool3x1Attribute;
            public static AttributeInfo bool3x2Attribute;
            public static AttributeInfo bool3x3Attribute;
            public static AttributeInfo bool3x4Attribute;
            public static AttributeInfo bool4x1Attribute;
            public static AttributeInfo bool4x2Attribute;
            public static AttributeInfo bool4x3Attribute;
            public static AttributeInfo bool4x4Attribute;
            public static AttributeInfo floatAttribute;
            public static AttributeInfo float1Attribute;
            public static AttributeInfo float2Attribute;
            public static AttributeInfo float3Attribute;
            public static AttributeInfo float4Attribute;
            public static AttributeInfo float1x1Attribute;
            public static AttributeInfo float1x2Attribute;
            public static AttributeInfo float1x3Attribute;
            public static AttributeInfo float1x4Attribute;
            public static AttributeInfo float2x1Attribute;
            public static AttributeInfo float2x2Attribute;
            public static AttributeInfo float2x3Attribute;
            public static AttributeInfo float2x4Attribute;
            public static AttributeInfo float3x1Attribute;
            public static AttributeInfo float3x2Attribute;
            public static AttributeInfo float3x3Attribute;
            public static AttributeInfo float3x4Attribute;
            public static AttributeInfo float4x1Attribute;
            public static AttributeInfo float4x2Attribute;
            public static AttributeInfo float4x3Attribute;
            public static AttributeInfo float4x4Attribute;
            public static AttributeInfo intAttribute;
            public static AttributeInfo int1Attribute;
            public static AttributeInfo int2Attribute;
            public static AttributeInfo int3Attribute;
            public static AttributeInfo int4Attribute;
            public static AttributeInfo int1x1Attribute;
            public static AttributeInfo int1x2Attribute;
            public static AttributeInfo int1x3Attribute;
            public static AttributeInfo int1x4Attribute;
            public static AttributeInfo int2x1Attribute;
            public static AttributeInfo int2x2Attribute;
            public static AttributeInfo int2x3Attribute;
            public static AttributeInfo int2x4Attribute;
            public static AttributeInfo int3x1Attribute;
            public static AttributeInfo int3x2Attribute;
            public static AttributeInfo int3x3Attribute;
            public static AttributeInfo int3x4Attribute;
            public static AttributeInfo int4x1Attribute;
            public static AttributeInfo int4x2Attribute;
            public static AttributeInfo int4x3Attribute;
            public static AttributeInfo int4x4Attribute;
            public static AttributeInfo halfAttribute;
            public static AttributeInfo half1Attribute;
            public static AttributeInfo half2Attribute;
            public static AttributeInfo half3Attribute;
            public static AttributeInfo half4Attribute;
            public static AttributeInfo half1x1Attribute;
            public static AttributeInfo half1x2Attribute;
            public static AttributeInfo half1x3Attribute;
            public static AttributeInfo half1x4Attribute;
            public static AttributeInfo half2x1Attribute;
            public static AttributeInfo half2x2Attribute;
            public static AttributeInfo half2x3Attribute;
            public static AttributeInfo half2x4Attribute;
            public static AttributeInfo half3x1Attribute;
            public static AttributeInfo half3x2Attribute;
            public static AttributeInfo half3x3Attribute;
            public static AttributeInfo half3x4Attribute;
            public static AttributeInfo half4x1Attribute;
            public static AttributeInfo half4x2Attribute;
            public static AttributeInfo half4x3Attribute;
            public static AttributeInfo half4x4Attribute;
            public static AttributeInfo fixedAttribute;
            public static AttributeInfo fixed1Attribute;
            public static AttributeInfo fixed2Attribute;
            public static AttributeInfo fixed3Attribute;
            public static AttributeInfo fixed4Attribute;
            public static AttributeInfo fixed1x1Attribute;
            public static AttributeInfo fixed1x2Attribute;
            public static AttributeInfo fixed1x3Attribute;
            public static AttributeInfo fixed1x4Attribute;
            public static AttributeInfo fixed2x1Attribute;
            public static AttributeInfo fixed2x2Attribute;
            public static AttributeInfo fixed2x3Attribute;
            public static AttributeInfo fixed2x4Attribute;
            public static AttributeInfo fixed3x1Attribute;
            public static AttributeInfo fixed3x2Attribute;
            public static AttributeInfo fixed3x3Attribute;
            public static AttributeInfo fixed3x4Attribute;
            public static AttributeInfo fixed4x1Attribute;
            public static AttributeInfo fixed4x2Attribute;
            public static AttributeInfo fixed4x3Attribute;
            public static AttributeInfo fixed4x4Attribute;
            public static AttributeInfo stringAttribute;
            public static AttributeInfo enumAttribute;
            public static AttributeInfo lengthAttribute;
            public static ChildInfo surfaceChild;
            public static ChildInfo sampler1DChild;
            public static ChildInfo sampler2DChild;
            public static ChildInfo sampler3DChild;
            public static ChildInfo samplerRECTChild;
            public static ChildInfo samplerCUBEChild;
            public static ChildInfo samplerDEPTHChild;
            public static ChildInfo arrayChild;
            public static ChildInfo usertypeChild;
            public static ChildInfo connect_paramChild;
        }

        public static class profile_CG_technique
        {
            public static DomNodeType Type;
            public static AttributeInfo idAttribute;
            public static AttributeInfo sidAttribute;
            public static ChildInfo assetChild;
            public static ChildInfo annotateChild;
            public static ChildInfo codeChild;
            public static ChildInfo includeChild;
            public static ChildInfo imageChild;
            public static ChildInfo newparamChild;
            public static ChildInfo setparamChild;
            public static ChildInfo passChild;
            public static ChildInfo extraChild;
        }

        public static class technique_pass
        {
            public static DomNodeType Type;
            public static AttributeInfo drawAttribute;
            public static AttributeInfo sidAttribute;
            public static ChildInfo annotateChild;
            public static ChildInfo color_targetChild;
            public static ChildInfo depth_targetChild;
            public static ChildInfo stencil_targetChild;
            public static ChildInfo color_clearChild;
            public static ChildInfo depth_clearChild;
            public static ChildInfo stencil_clearChild;
            public static ChildInfo alpha_funcChild;
            public static ChildInfo blend_funcChild;
            public static ChildInfo blend_func_separateChild;
            public static ChildInfo blend_equationChild;
            public static ChildInfo blend_equation_separateChild;
            public static ChildInfo color_materialChild;
            public static ChildInfo cull_faceChild;
            public static ChildInfo depth_funcChild;
            public static ChildInfo fog_modeChild;
            public static ChildInfo fog_coord_srcChild;
            public static ChildInfo front_faceChild;
            public static ChildInfo light_model_color_controlChild;
            public static ChildInfo logic_opChild;
            public static ChildInfo polygon_modeChild;
            public static ChildInfo shade_modelChild;
            public static ChildInfo stencil_funcChild;
            public static ChildInfo stencil_opChild;
            public static ChildInfo stencil_func_separateChild;
            public static ChildInfo stencil_op_separateChild;
            public static ChildInfo stencil_mask_separateChild;
            public static ChildInfo light_enableChild;
            public static ChildInfo light_ambientChild;
            public static ChildInfo light_diffuseChild;
            public static ChildInfo light_specularChild;
            public static ChildInfo light_positionChild;
            public static ChildInfo light_constant_attenuationChild;
            public static ChildInfo light_linear_attenuationChild;
            public static ChildInfo light_quadratic_attenuationChild;
            public static ChildInfo light_spot_cutoffChild;
            public static ChildInfo light_spot_directionChild;
            public static ChildInfo light_spot_exponentChild;
            public static ChildInfo texture1DChild;
            public static ChildInfo texture2DChild;
            public static ChildInfo texture3DChild;
            public static ChildInfo textureCUBEChild;
            public static ChildInfo textureRECTChild;
            public static ChildInfo textureDEPTHChild;
            public static ChildInfo texture1D_enableChild;
            public static ChildInfo texture2D_enableChild;
            public static ChildInfo texture3D_enableChild;
            public static ChildInfo textureCUBE_enableChild;
            public static ChildInfo textureRECT_enableChild;
            public static ChildInfo textureDEPTH_enableChild;
            public static ChildInfo texture_env_colorChild;
            public static ChildInfo texture_env_modeChild;
            public static ChildInfo clip_planeChild;
            public static ChildInfo clip_plane_enableChild;
            public static ChildInfo blend_colorChild;
            public static ChildInfo clear_colorChild;
            public static ChildInfo clear_stencilChild;
            public static ChildInfo clear_depthChild;
            public static ChildInfo color_maskChild;
            public static ChildInfo depth_boundsChild;
            public static ChildInfo depth_maskChild;
            public static ChildInfo depth_rangeChild;
            public static ChildInfo fog_densityChild;
            public static ChildInfo fog_startChild;
            public static ChildInfo fog_endChild;
            public static ChildInfo fog_colorChild;
            public static ChildInfo light_model_ambientChild;
            public static ChildInfo lighting_enableChild;
            public static ChildInfo line_stippleChild;
            public static ChildInfo line_widthChild;
            public static ChildInfo material_ambientChild;
            public static ChildInfo material_diffuseChild;
            public static ChildInfo material_emissionChild;
            public static ChildInfo material_shininessChild;
            public static ChildInfo material_specularChild;
            public static ChildInfo model_view_matrixChild;
            public static ChildInfo point_distance_attenuationChild;
            public static ChildInfo point_fade_threshold_sizeChild;
            public static ChildInfo point_sizeChild;
            public static ChildInfo point_size_minChild;
            public static ChildInfo point_size_maxChild;
            public static ChildInfo polygon_offsetChild;
            public static ChildInfo projection_matrixChild;
            public static ChildInfo scissorChild;
            public static ChildInfo stencil_maskChild;
            public static ChildInfo alpha_test_enableChild;
            public static ChildInfo auto_normal_enableChild;
            public static ChildInfo blend_enableChild;
            public static ChildInfo color_logic_op_enableChild;
            public static ChildInfo color_material_enableChild;
            public static ChildInfo cull_face_enableChild;
            public static ChildInfo depth_bounds_enableChild;
            public static ChildInfo depth_clamp_enableChild;
            public static ChildInfo depth_test_enableChild;
            public static ChildInfo dither_enableChild;
            public static ChildInfo fog_enableChild;
            public static ChildInfo light_model_local_viewer_enableChild;
            public static ChildInfo light_model_two_side_enableChild;
            public static ChildInfo line_smooth_enableChild;
            public static ChildInfo line_stipple_enableChild;
            public static ChildInfo logic_op_enableChild;
            public static ChildInfo multisample_enableChild;
            public static ChildInfo normalize_enableChild;
            public static ChildInfo point_smooth_enableChild;
            public static ChildInfo polygon_offset_fill_enableChild;
            public static ChildInfo polygon_offset_line_enableChild;
            public static ChildInfo polygon_offset_point_enableChild;
            public static ChildInfo polygon_smooth_enableChild;
            public static ChildInfo polygon_stipple_enableChild;
            public static ChildInfo rescale_normal_enableChild;
            public static ChildInfo sample_alpha_to_coverage_enableChild;
            public static ChildInfo sample_alpha_to_one_enableChild;
            public static ChildInfo sample_coverage_enableChild;
            public static ChildInfo scissor_test_enableChild;
            public static ChildInfo stencil_test_enableChild;
            public static ChildInfo gl_hook_abstractChild;
            public static ChildInfo shaderChild;
            public static ChildInfo extraChild;
        }

        public static class fx_colortarget_common
        {
            public static DomNodeType Type;
            public static AttributeInfo Attribute;
            public static AttributeInfo indexAttribute;
            public static AttributeInfo faceAttribute;
            public static AttributeInfo mipAttribute;
            public static AttributeInfo sliceAttribute;
        }

        public static class fx_depthtarget_common
        {
            public static DomNodeType Type;
            public static AttributeInfo Attribute;
            public static AttributeInfo indexAttribute;
            public static AttributeInfo faceAttribute;
            public static AttributeInfo mipAttribute;
            public static AttributeInfo sliceAttribute;
        }

        public static class fx_stenciltarget_common
        {
            public static DomNodeType Type;
            public static AttributeInfo Attribute;
            public static AttributeInfo indexAttribute;
            public static AttributeInfo faceAttribute;
            public static AttributeInfo mipAttribute;
            public static AttributeInfo sliceAttribute;
        }

        public static class fx_clearcolor_common
        {
            public static DomNodeType Type;
            public static AttributeInfo Attribute;
            public static AttributeInfo indexAttribute;
        }

        public static class fx_cleardepth_common
        {
            public static DomNodeType Type;
            public static AttributeInfo Attribute;
            public static AttributeInfo indexAttribute;
        }

        public static class fx_clearstencil_common
        {
            public static DomNodeType Type;
            public static AttributeInfo Attribute;
            public static AttributeInfo indexAttribute;
        }

        public static class alpha_func
        {
            public static DomNodeType Type;
            public static ChildInfo funcChild;
            public static ChildInfo valueChild;
        }

        public static class alpha_func_func
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class alpha_func_value
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class blend_func
        {
            public static DomNodeType Type;
            public static ChildInfo srcChild;
            public static ChildInfo destChild;
        }

        public static class blend_func_src
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class blend_func_dest
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class blend_func_separate
        {
            public static DomNodeType Type;
            public static ChildInfo src_rgbChild;
            public static ChildInfo dest_rgbChild;
            public static ChildInfo src_alphaChild;
            public static ChildInfo dest_alphaChild;
        }

        public static class blend_func_separate_src_rgb
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class blend_func_separate_dest_rgb
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class blend_func_separate_src_alpha
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class blend_func_separate_dest_alpha
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class blend_equation
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class blend_equation_separate
        {
            public static DomNodeType Type;
            public static ChildInfo rgbChild;
            public static ChildInfo alphaChild;
        }

        public static class blend_equation_separate_rgb
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class blend_equation_separate_alpha
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class color_material
        {
            public static DomNodeType Type;
            public static ChildInfo faceChild;
            public static ChildInfo modeChild;
        }

        public static class color_material_face
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class color_material_mode
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class cull_face
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class depth_func
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class fog_mode
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class fog_coord_src
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class front_face
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class light_model_color_control
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class logic_op
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class polygon_mode
        {
            public static DomNodeType Type;
            public static ChildInfo faceChild;
            public static ChildInfo modeChild;
        }

        public static class polygon_mode_face
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class polygon_mode_mode
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class shade_model
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class stencil_func
        {
            public static DomNodeType Type;
            public static ChildInfo funcChild;
            public static ChildInfo refChild;
            public static ChildInfo maskChild;
        }

        public static class stencil_func_func
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class stencil_func_ref
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class stencil_func_mask
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class stencil_op
        {
            public static DomNodeType Type;
            public static ChildInfo failChild;
            public static ChildInfo zfailChild;
            public static ChildInfo zpassChild;
        }

        public static class stencil_op_fail
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class stencil_op_zfail
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class stencil_op_zpass
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class stencil_func_separate
        {
            public static DomNodeType Type;
            public static ChildInfo frontChild;
            public static ChildInfo backChild;
            public static ChildInfo refChild;
            public static ChildInfo maskChild;
        }

        public static class stencil_func_separate_front
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class stencil_func_separate_back
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class stencil_func_separate_ref
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class stencil_func_separate_mask
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class stencil_op_separate
        {
            public static DomNodeType Type;
            public static ChildInfo faceChild;
            public static ChildInfo failChild;
            public static ChildInfo zfailChild;
            public static ChildInfo zpassChild;
        }

        public static class stencil_op_separate_face
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class stencil_op_separate_fail
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class stencil_op_separate_zfail
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class stencil_op_separate_zpass
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class stencil_mask_separate
        {
            public static DomNodeType Type;
            public static ChildInfo faceChild;
            public static ChildInfo maskChild;
        }

        public static class stencil_mask_separate_face
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class stencil_mask_separate_mask
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class light_enable
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
            public static AttributeInfo indexAttribute;
        }

        public static class light_ambient
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
            public static AttributeInfo indexAttribute;
        }

        public static class light_diffuse
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
            public static AttributeInfo indexAttribute;
        }

        public static class light_specular
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
            public static AttributeInfo indexAttribute;
        }

        public static class light_position
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
            public static AttributeInfo indexAttribute;
        }

        public static class light_constant_attenuation
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
            public static AttributeInfo indexAttribute;
        }

        public static class light_linear_attenuation
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
            public static AttributeInfo indexAttribute;
        }

        public static class light_quadratic_attenuation
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
            public static AttributeInfo indexAttribute;
        }

        public static class light_spot_cutoff
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
            public static AttributeInfo indexAttribute;
        }

        public static class light_spot_direction
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
            public static AttributeInfo indexAttribute;
        }

        public static class light_spot_exponent
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
            public static AttributeInfo indexAttribute;
        }

        public static class texture1D
        {
            public static DomNodeType Type;
            public static AttributeInfo paramAttribute;
            public static AttributeInfo indexAttribute;
            public static ChildInfo valueChild;
        }

        public static class gl_sampler1D
        {
            public static DomNodeType Type;
            public static AttributeInfo sourceAttribute;
            public static AttributeInfo wrap_sAttribute;
            public static AttributeInfo minfilterAttribute;
            public static AttributeInfo magfilterAttribute;
            public static AttributeInfo mipfilterAttribute;
            public static AttributeInfo border_colorAttribute;
            public static AttributeInfo mipmap_maxlevelAttribute;
            public static AttributeInfo mipmap_biasAttribute;
            public static ChildInfo extraChild;
        }

        public static class texture2D
        {
            public static DomNodeType Type;
            public static AttributeInfo paramAttribute;
            public static AttributeInfo indexAttribute;
            public static ChildInfo valueChild;
        }

        public static class gl_sampler2D
        {
            public static DomNodeType Type;
            public static AttributeInfo sourceAttribute;
            public static AttributeInfo wrap_sAttribute;
            public static AttributeInfo wrap_tAttribute;
            public static AttributeInfo minfilterAttribute;
            public static AttributeInfo magfilterAttribute;
            public static AttributeInfo mipfilterAttribute;
            public static AttributeInfo border_colorAttribute;
            public static AttributeInfo mipmap_maxlevelAttribute;
            public static AttributeInfo mipmap_biasAttribute;
            public static ChildInfo extraChild;
        }

        public static class texture3D
        {
            public static DomNodeType Type;
            public static AttributeInfo paramAttribute;
            public static AttributeInfo indexAttribute;
            public static ChildInfo valueChild;
        }

        public static class gl_sampler3D
        {
            public static DomNodeType Type;
            public static AttributeInfo sourceAttribute;
            public static AttributeInfo wrap_sAttribute;
            public static AttributeInfo wrap_tAttribute;
            public static AttributeInfo wrap_pAttribute;
            public static AttributeInfo minfilterAttribute;
            public static AttributeInfo magfilterAttribute;
            public static AttributeInfo mipfilterAttribute;
            public static AttributeInfo border_colorAttribute;
            public static AttributeInfo mipmap_maxlevelAttribute;
            public static AttributeInfo mipmap_biasAttribute;
            public static ChildInfo extraChild;
        }

        public static class textureCUBE
        {
            public static DomNodeType Type;
            public static AttributeInfo paramAttribute;
            public static AttributeInfo indexAttribute;
            public static ChildInfo valueChild;
        }

        public static class gl_samplerCUBE
        {
            public static DomNodeType Type;
            public static AttributeInfo sourceAttribute;
            public static AttributeInfo wrap_sAttribute;
            public static AttributeInfo wrap_tAttribute;
            public static AttributeInfo wrap_pAttribute;
            public static AttributeInfo minfilterAttribute;
            public static AttributeInfo magfilterAttribute;
            public static AttributeInfo mipfilterAttribute;
            public static AttributeInfo border_colorAttribute;
            public static AttributeInfo mipmap_maxlevelAttribute;
            public static AttributeInfo mipmap_biasAttribute;
            public static ChildInfo extraChild;
        }

        public static class textureRECT
        {
            public static DomNodeType Type;
            public static AttributeInfo paramAttribute;
            public static AttributeInfo indexAttribute;
            public static ChildInfo valueChild;
        }

        public static class gl_samplerRECT
        {
            public static DomNodeType Type;
            public static AttributeInfo sourceAttribute;
            public static AttributeInfo wrap_sAttribute;
            public static AttributeInfo wrap_tAttribute;
            public static AttributeInfo minfilterAttribute;
            public static AttributeInfo magfilterAttribute;
            public static AttributeInfo mipfilterAttribute;
            public static AttributeInfo border_colorAttribute;
            public static AttributeInfo mipmap_maxlevelAttribute;
            public static AttributeInfo mipmap_biasAttribute;
            public static ChildInfo extraChild;
        }

        public static class textureDEPTH
        {
            public static DomNodeType Type;
            public static AttributeInfo paramAttribute;
            public static AttributeInfo indexAttribute;
            public static ChildInfo valueChild;
        }

        public static class gl_samplerDEPTH
        {
            public static DomNodeType Type;
            public static AttributeInfo sourceAttribute;
            public static AttributeInfo wrap_sAttribute;
            public static AttributeInfo wrap_tAttribute;
            public static AttributeInfo minfilterAttribute;
            public static AttributeInfo magfilterAttribute;
            public static ChildInfo extraChild;
        }

        public static class texture1D_enable
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
            public static AttributeInfo indexAttribute;
        }

        public static class texture2D_enable
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
            public static AttributeInfo indexAttribute;
        }

        public static class texture3D_enable
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
            public static AttributeInfo indexAttribute;
        }

        public static class textureCUBE_enable
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
            public static AttributeInfo indexAttribute;
        }

        public static class textureRECT_enable
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
            public static AttributeInfo indexAttribute;
        }

        public static class textureDEPTH_enable
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
            public static AttributeInfo indexAttribute;
        }

        public static class texture_env_color
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
            public static AttributeInfo indexAttribute;
        }

        public static class texture_env_mode
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
            public static AttributeInfo indexAttribute;
        }

        public static class clip_plane
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
            public static AttributeInfo indexAttribute;
        }

        public static class clip_plane_enable
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
            public static AttributeInfo indexAttribute;
        }

        public static class blend_color
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class clear_color
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class clear_stencil
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class clear_depth
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class color_mask
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class depth_bounds
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class depth_mask
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class depth_range
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class fog_density
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class fog_start
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class fog_end
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class fog_color
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class light_model_ambient
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class lighting_enable
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class line_stipple
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class line_width
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class material_ambient
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class material_diffuse
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class material_emission
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class material_shininess
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class material_specular
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class model_view_matrix
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class point_distance_attenuation
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class point_fade_threshold_size
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class point_size
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class point_size_min
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class point_size_max
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class polygon_offset
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class projection_matrix
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class scissor
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class stencil_mask
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class alpha_test_enable
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class auto_normal_enable
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class blend_enable
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class color_logic_op_enable
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class color_material_enable
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class cull_face_enable
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class depth_bounds_enable
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class depth_clamp_enable
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class depth_test_enable
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class dither_enable
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class fog_enable
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class light_model_local_viewer_enable
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class light_model_two_side_enable
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class line_smooth_enable
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class line_stipple_enable
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class logic_op_enable
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class multisample_enable
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class normalize_enable
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class point_smooth_enable
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class polygon_offset_fill_enable
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class polygon_offset_line_enable
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class polygon_offset_point_enable
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class polygon_smooth_enable
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class polygon_stipple_enable
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class rescale_normal_enable
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class sample_alpha_to_coverage_enable
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class sample_alpha_to_one_enable
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class sample_coverage_enable
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class scissor_test_enable
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class stencil_test_enable
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class pass_shader
        {
            public static DomNodeType Type;
            public static AttributeInfo compiler_optionsAttribute;
            public static AttributeInfo stageAttribute;
            public static ChildInfo annotateChild;
            public static ChildInfo compiler_targetChild;
            public static ChildInfo nameChild;
            public static ChildInfo bindChild;
        }

        public static class shader_compiler_target
        {
            public static DomNodeType Type;
            public static AttributeInfo Attribute;
        }

        public static class shader_name
        {
            public static DomNodeType Type;
            public static AttributeInfo Attribute;
            public static AttributeInfo sourceAttribute;
        }

        public static class shader_bind
        {
            public static DomNodeType Type;
            public static AttributeInfo boolAttribute;
            public static AttributeInfo bool1Attribute;
            public static AttributeInfo bool2Attribute;
            public static AttributeInfo bool3Attribute;
            public static AttributeInfo bool4Attribute;
            public static AttributeInfo bool1x1Attribute;
            public static AttributeInfo bool1x2Attribute;
            public static AttributeInfo bool1x3Attribute;
            public static AttributeInfo bool1x4Attribute;
            public static AttributeInfo bool2x1Attribute;
            public static AttributeInfo bool2x2Attribute;
            public static AttributeInfo bool2x3Attribute;
            public static AttributeInfo bool2x4Attribute;
            public static AttributeInfo bool3x1Attribute;
            public static AttributeInfo bool3x2Attribute;
            public static AttributeInfo bool3x3Attribute;
            public static AttributeInfo bool3x4Attribute;
            public static AttributeInfo bool4x1Attribute;
            public static AttributeInfo bool4x2Attribute;
            public static AttributeInfo bool4x3Attribute;
            public static AttributeInfo bool4x4Attribute;
            public static AttributeInfo floatAttribute;
            public static AttributeInfo float1Attribute;
            public static AttributeInfo float2Attribute;
            public static AttributeInfo float3Attribute;
            public static AttributeInfo float4Attribute;
            public static AttributeInfo float1x1Attribute;
            public static AttributeInfo float1x2Attribute;
            public static AttributeInfo float1x3Attribute;
            public static AttributeInfo float1x4Attribute;
            public static AttributeInfo float2x1Attribute;
            public static AttributeInfo float2x2Attribute;
            public static AttributeInfo float2x3Attribute;
            public static AttributeInfo float2x4Attribute;
            public static AttributeInfo float3x1Attribute;
            public static AttributeInfo float3x2Attribute;
            public static AttributeInfo float3x3Attribute;
            public static AttributeInfo float3x4Attribute;
            public static AttributeInfo float4x1Attribute;
            public static AttributeInfo float4x2Attribute;
            public static AttributeInfo float4x3Attribute;
            public static AttributeInfo float4x4Attribute;
            public static AttributeInfo intAttribute;
            public static AttributeInfo int1Attribute;
            public static AttributeInfo int2Attribute;
            public static AttributeInfo int3Attribute;
            public static AttributeInfo int4Attribute;
            public static AttributeInfo int1x1Attribute;
            public static AttributeInfo int1x2Attribute;
            public static AttributeInfo int1x3Attribute;
            public static AttributeInfo int1x4Attribute;
            public static AttributeInfo int2x1Attribute;
            public static AttributeInfo int2x2Attribute;
            public static AttributeInfo int2x3Attribute;
            public static AttributeInfo int2x4Attribute;
            public static AttributeInfo int3x1Attribute;
            public static AttributeInfo int3x2Attribute;
            public static AttributeInfo int3x3Attribute;
            public static AttributeInfo int3x4Attribute;
            public static AttributeInfo int4x1Attribute;
            public static AttributeInfo int4x2Attribute;
            public static AttributeInfo int4x3Attribute;
            public static AttributeInfo int4x4Attribute;
            public static AttributeInfo halfAttribute;
            public static AttributeInfo half1Attribute;
            public static AttributeInfo half2Attribute;
            public static AttributeInfo half3Attribute;
            public static AttributeInfo half4Attribute;
            public static AttributeInfo half1x1Attribute;
            public static AttributeInfo half1x2Attribute;
            public static AttributeInfo half1x3Attribute;
            public static AttributeInfo half1x4Attribute;
            public static AttributeInfo half2x1Attribute;
            public static AttributeInfo half2x2Attribute;
            public static AttributeInfo half2x3Attribute;
            public static AttributeInfo half2x4Attribute;
            public static AttributeInfo half3x1Attribute;
            public static AttributeInfo half3x2Attribute;
            public static AttributeInfo half3x3Attribute;
            public static AttributeInfo half3x4Attribute;
            public static AttributeInfo half4x1Attribute;
            public static AttributeInfo half4x2Attribute;
            public static AttributeInfo half4x3Attribute;
            public static AttributeInfo half4x4Attribute;
            public static AttributeInfo fixedAttribute;
            public static AttributeInfo fixed1Attribute;
            public static AttributeInfo fixed2Attribute;
            public static AttributeInfo fixed3Attribute;
            public static AttributeInfo fixed4Attribute;
            public static AttributeInfo fixed1x1Attribute;
            public static AttributeInfo fixed1x2Attribute;
            public static AttributeInfo fixed1x3Attribute;
            public static AttributeInfo fixed1x4Attribute;
            public static AttributeInfo fixed2x1Attribute;
            public static AttributeInfo fixed2x2Attribute;
            public static AttributeInfo fixed2x3Attribute;
            public static AttributeInfo fixed2x4Attribute;
            public static AttributeInfo fixed3x1Attribute;
            public static AttributeInfo fixed3x2Attribute;
            public static AttributeInfo fixed3x3Attribute;
            public static AttributeInfo fixed3x4Attribute;
            public static AttributeInfo fixed4x1Attribute;
            public static AttributeInfo fixed4x2Attribute;
            public static AttributeInfo fixed4x3Attribute;
            public static AttributeInfo fixed4x4Attribute;
            public static AttributeInfo stringAttribute;
            public static AttributeInfo enumAttribute;
            public static AttributeInfo symbolAttribute;
            public static ChildInfo surfaceChild;
            public static ChildInfo sampler1DChild;
            public static ChildInfo sampler2DChild;
            public static ChildInfo sampler3DChild;
            public static ChildInfo samplerRECTChild;
            public static ChildInfo samplerCUBEChild;
            public static ChildInfo samplerDEPTHChild;
            public static ChildInfo paramChild;
        }

        public static class bind_param
        {
            public static DomNodeType Type;
            public static AttributeInfo refAttribute;
        }

        public static class profile_COMMON
        {
            public static DomNodeType Type;
            public static AttributeInfo idAttribute;
            public static ChildInfo assetChild;
            public static ChildInfo imageChild;
            public static ChildInfo newparamChild;
            public static ChildInfo techniqueChild;
            public static ChildInfo extraChild;
        }

        public static class common_newparam_type
        {
            public static DomNodeType Type;
            public static AttributeInfo semanticAttribute;
            public static AttributeInfo floatAttribute;
            public static AttributeInfo float2Attribute;
            public static AttributeInfo float3Attribute;
            public static AttributeInfo float4Attribute;
            public static AttributeInfo sidAttribute;
            public static ChildInfo surfaceChild;
            public static ChildInfo sampler2DChild;
        }

        public static class profile_COMMON_technique
        {
            public static DomNodeType Type;
            public static AttributeInfo idAttribute;
            public static AttributeInfo sidAttribute;
            public static ChildInfo assetChild;
            public static ChildInfo imageChild;
            public static ChildInfo newparamChild;
            public static ChildInfo constantChild;
            public static ChildInfo lambertChild;
            public static ChildInfo phongChild;
            public static ChildInfo blinnChild;
            public static ChildInfo extraChild;
        }

        public static class technique_constant
        {
            public static DomNodeType Type;
            public static ChildInfo emissionChild;
            public static ChildInfo reflectiveChild;
            public static ChildInfo reflectivityChild;
            public static ChildInfo transparentChild;
            public static ChildInfo transparencyChild;
            public static ChildInfo index_of_refractionChild;
        }

        public static class common_color_or_texture_type
        {
            public static DomNodeType Type;
            public static ChildInfo colorChild;
            public static ChildInfo paramChild;
            public static ChildInfo textureChild;
        }

        public static class common_color_or_texture_type_color
        {
            public static DomNodeType Type;
            public static AttributeInfo Attribute;
            public static AttributeInfo sidAttribute;
        }

        public static class common_color_or_texture_type_param
        {
            public static DomNodeType Type;
            public static AttributeInfo refAttribute;
        }

        public static class common_color_or_texture_type_texture
        {
            public static DomNodeType Type;
            public static AttributeInfo textureAttribute;
            public static AttributeInfo texcoordAttribute;
            public static ChildInfo extraChild;
        }

        public static class common_float_or_param_type
        {
            public static DomNodeType Type;
            public static ChildInfo floatChild;
            public static ChildInfo paramChild;
        }

        public static class common_float_or_param_type_float
        {
            public static DomNodeType Type;
            public static AttributeInfo Attribute;
            public static AttributeInfo sidAttribute;
        }

        public static class common_float_or_param_type_param
        {
            public static DomNodeType Type;
            public static AttributeInfo refAttribute;
        }

        public static class common_transparent_type
        {
            public static DomNodeType Type;
            public static AttributeInfo opaqueAttribute;
            public static ChildInfo colorChild;
            public static ChildInfo paramChild;
            public static ChildInfo textureChild;
        }

        public static class technique_lambert
        {
            public static DomNodeType Type;
            public static ChildInfo emissionChild;
            public static ChildInfo ambientChild;
            public static ChildInfo diffuseChild;
            public static ChildInfo reflectiveChild;
            public static ChildInfo reflectivityChild;
            public static ChildInfo transparentChild;
            public static ChildInfo transparencyChild;
            public static ChildInfo index_of_refractionChild;
        }

        public static class technique_phong
        {
            public static DomNodeType Type;
            public static ChildInfo emissionChild;
            public static ChildInfo ambientChild;
            public static ChildInfo diffuseChild;
            public static ChildInfo specularChild;
            public static ChildInfo shininessChild;
            public static ChildInfo reflectiveChild;
            public static ChildInfo reflectivityChild;
            public static ChildInfo transparentChild;
            public static ChildInfo transparencyChild;
            public static ChildInfo index_of_refractionChild;
        }

        public static class technique_blinn
        {
            public static DomNodeType Type;
            public static ChildInfo emissionChild;
            public static ChildInfo ambientChild;
            public static ChildInfo diffuseChild;
            public static ChildInfo specularChild;
            public static ChildInfo shininessChild;
            public static ChildInfo reflectiveChild;
            public static ChildInfo reflectivityChild;
            public static ChildInfo transparentChild;
            public static ChildInfo transparencyChild;
            public static ChildInfo index_of_refractionChild;
        }

        public static class library_force_fields
        {
            public static DomNodeType Type;
            public static AttributeInfo idAttribute;
            public static AttributeInfo nameAttribute;
            public static ChildInfo assetChild;
            public static ChildInfo force_fieldChild;
            public static ChildInfo extraChild;
        }

        public static class force_field
        {
            public static DomNodeType Type;
            public static AttributeInfo idAttribute;
            public static AttributeInfo nameAttribute;
            public static ChildInfo assetChild;
            public static ChildInfo techniqueChild;
            public static ChildInfo extraChild;
        }

        public static class library_images
        {
            public static DomNodeType Type;
            public static AttributeInfo idAttribute;
            public static AttributeInfo nameAttribute;
            public static ChildInfo assetChild;
            public static ChildInfo imageChild;
            public static ChildInfo extraChild;
        }

        public static class library_lights
        {
            public static DomNodeType Type;
            public static AttributeInfo idAttribute;
            public static AttributeInfo nameAttribute;
            public static ChildInfo assetChild;
            public static ChildInfo lightChild;
            public static ChildInfo extraChild;
        }

        public static class light
        {
            public static DomNodeType Type;
            public static AttributeInfo idAttribute;
            public static AttributeInfo nameAttribute;
            public static ChildInfo assetChild;
            public static ChildInfo technique_commonChild;
            public static ChildInfo techniqueChild;
            public static ChildInfo extraChild;
        }

        public static class light_technique_common
        {
            public static DomNodeType Type;
            public static ChildInfo ambientChild;
            public static ChildInfo directionalChild;
            public static ChildInfo pointChild;
            public static ChildInfo spotChild;
        }

        public static class technique_common_ambient
        {
            public static DomNodeType Type;
            public static ChildInfo colorChild;
        }

        public static class TargetableFloat3
        {
            public static DomNodeType Type;
            public static AttributeInfo Attribute;
            public static AttributeInfo sidAttribute;
        }

        public static class technique_common_directional
        {
            public static DomNodeType Type;
            public static ChildInfo colorChild;
        }

        public static class technique_common_point
        {
            public static DomNodeType Type;
            public static ChildInfo colorChild;
            public static ChildInfo constant_attenuationChild;
            public static ChildInfo linear_attenuationChild;
            public static ChildInfo quadratic_attenuationChild;
        }

        public static class technique_common_spot
        {
            public static DomNodeType Type;
            public static ChildInfo colorChild;
            public static ChildInfo constant_attenuationChild;
            public static ChildInfo linear_attenuationChild;
            public static ChildInfo quadratic_attenuationChild;
            public static ChildInfo falloff_angleChild;
            public static ChildInfo falloff_exponentChild;
        }

        public static class library_materials
        {
            public static DomNodeType Type;
            public static AttributeInfo idAttribute;
            public static AttributeInfo nameAttribute;
            public static ChildInfo assetChild;
            public static ChildInfo materialChild;
            public static ChildInfo extraChild;
        }

        public static class material
        {
            public static DomNodeType Type;
            public static AttributeInfo idAttribute;
            public static AttributeInfo nameAttribute;
            public static ChildInfo assetChild;
            public static ChildInfo instance_effectChild;
            public static ChildInfo extraChild;
        }

        public static class instance_effect
        {
            public static DomNodeType Type;
            public static AttributeInfo urlAttribute;
            public static AttributeInfo sidAttribute;
            public static AttributeInfo nameAttribute;
            public static ChildInfo technique_hintChild;
            public static ChildInfo setparamChild;
            public static ChildInfo extraChild;
        }

        public static class instance_effect_technique_hint
        {
            public static DomNodeType Type;
            public static AttributeInfo platformAttribute;
            public static AttributeInfo profileAttribute;
            public static AttributeInfo refAttribute;
        }

        public static class instance_effect_setparam
        {
            public static DomNodeType Type;
            public static AttributeInfo boolAttribute;
            public static AttributeInfo bool2Attribute;
            public static AttributeInfo bool3Attribute;
            public static AttributeInfo bool4Attribute;
            public static AttributeInfo intAttribute;
            public static AttributeInfo int2Attribute;
            public static AttributeInfo int3Attribute;
            public static AttributeInfo int4Attribute;
            public static AttributeInfo floatAttribute;
            public static AttributeInfo float2Attribute;
            public static AttributeInfo float3Attribute;
            public static AttributeInfo float4Attribute;
            public static AttributeInfo float1x1Attribute;
            public static AttributeInfo float1x2Attribute;
            public static AttributeInfo float1x3Attribute;
            public static AttributeInfo float1x4Attribute;
            public static AttributeInfo float2x1Attribute;
            public static AttributeInfo float2x2Attribute;
            public static AttributeInfo float2x3Attribute;
            public static AttributeInfo float2x4Attribute;
            public static AttributeInfo float3x1Attribute;
            public static AttributeInfo float3x2Attribute;
            public static AttributeInfo float3x3Attribute;
            public static AttributeInfo float3x4Attribute;
            public static AttributeInfo float4x1Attribute;
            public static AttributeInfo float4x2Attribute;
            public static AttributeInfo float4x3Attribute;
            public static AttributeInfo float4x4Attribute;
            public static AttributeInfo enumAttribute;
            public static AttributeInfo refAttribute;
            public static ChildInfo surfaceChild;
            public static ChildInfo sampler1DChild;
            public static ChildInfo sampler2DChild;
            public static ChildInfo sampler3DChild;
            public static ChildInfo samplerCUBEChild;
            public static ChildInfo samplerRECTChild;
            public static ChildInfo samplerDEPTHChild;
        }

        public static class library_nodes
        {
            public static DomNodeType Type;
            public static AttributeInfo idAttribute;
            public static AttributeInfo nameAttribute;
            public static ChildInfo assetChild;
            public static ChildInfo nodeChild;
            public static ChildInfo extraChild;
        }

        public static class node
        {
            public static DomNodeType Type;
            public static AttributeInfo idAttribute;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo sidAttribute;
            public static AttributeInfo typeAttribute;
            public static AttributeInfo layerAttribute;
            public static ChildInfo assetChild;
            public static ChildInfo lookatChild;
            public static ChildInfo matrixChild;
            public static ChildInfo rotateChild;
            public static ChildInfo scaleChild;
            public static ChildInfo skewChild;
            public static ChildInfo translateChild;
            public static ChildInfo instance_cameraChild;
            public static ChildInfo instance_controllerChild;
            public static ChildInfo instance_geometryChild;
            public static ChildInfo instance_lightChild;
            public static ChildInfo instance_nodeChild;
            public static ChildInfo nodeChild;
            public static ChildInfo extraChild;
        }

        public static class lookat
        {
            public static DomNodeType Type;
            public static AttributeInfo Attribute;
            public static AttributeInfo sidAttribute;
        }

        public static class matrix
        {
            public static DomNodeType Type;
            public static AttributeInfo Attribute;
            public static AttributeInfo sidAttribute;
        }

        public static class rotate
        {
            public static DomNodeType Type;
            public static AttributeInfo Attribute;
            public static AttributeInfo sidAttribute;
        }

        public static class skew
        {
            public static DomNodeType Type;
            public static AttributeInfo Attribute;
            public static AttributeInfo sidAttribute;
        }

        public static class instance_controller
        {
            public static DomNodeType Type;
            public static AttributeInfo urlAttribute;
            public static AttributeInfo sidAttribute;
            public static AttributeInfo nameAttribute;
            public static ChildInfo skeletonChild;
            public static ChildInfo bind_materialChild;
            public static ChildInfo extraChild;
        }

        public static class bind_material
        {
            public static DomNodeType Type;
            public static ChildInfo paramChild;
            public static ChildInfo technique_commonChild;
            public static ChildInfo techniqueChild;
            public static ChildInfo extraChild;
        }

        public static class bind_material_technique_common
        {
            public static DomNodeType Type;
            public static ChildInfo instance_materialChild;
        }

        public static class instance_material
        {
            public static DomNodeType Type;
            public static AttributeInfo symbolAttribute;
            public static AttributeInfo targetAttribute;
            public static AttributeInfo sidAttribute;
            public static AttributeInfo nameAttribute;
            public static ChildInfo bindChild;
            public static ChildInfo bind_vertex_inputChild;
            public static ChildInfo extraChild;
        }

        public static class instance_material_bind
        {
            public static DomNodeType Type;
            public static AttributeInfo semanticAttribute;
            public static AttributeInfo targetAttribute;
        }

        public static class instance_material_bind_vertex_input
        {
            public static DomNodeType Type;
            public static AttributeInfo semanticAttribute;
            public static AttributeInfo input_semanticAttribute;
            public static AttributeInfo input_setAttribute;
        }

        public static class instance_geometry
        {
            public static DomNodeType Type;
            public static AttributeInfo urlAttribute;
            public static AttributeInfo sidAttribute;
            public static AttributeInfo nameAttribute;
            public static ChildInfo bind_materialChild;
            public static ChildInfo extraChild;
        }

        public static class library_physics_materials
        {
            public static DomNodeType Type;
            public static AttributeInfo idAttribute;
            public static AttributeInfo nameAttribute;
            public static ChildInfo assetChild;
            public static ChildInfo physics_materialChild;
            public static ChildInfo extraChild;
        }

        public static class physics_material
        {
            public static DomNodeType Type;
            public static AttributeInfo idAttribute;
            public static AttributeInfo nameAttribute;
            public static ChildInfo assetChild;
            public static ChildInfo technique_commonChild;
            public static ChildInfo techniqueChild;
            public static ChildInfo extraChild;
        }

        public static class physics_material_technique_common
        {
            public static DomNodeType Type;
            public static ChildInfo dynamic_frictionChild;
            public static ChildInfo restitutionChild;
            public static ChildInfo static_frictionChild;
        }

        public static class library_physics_models
        {
            public static DomNodeType Type;
            public static AttributeInfo idAttribute;
            public static AttributeInfo nameAttribute;
            public static ChildInfo assetChild;
            public static ChildInfo physics_modelChild;
            public static ChildInfo extraChild;
        }

        public static class physics_model
        {
            public static DomNodeType Type;
            public static AttributeInfo idAttribute;
            public static AttributeInfo nameAttribute;
            public static ChildInfo assetChild;
            public static ChildInfo rigid_bodyChild;
            public static ChildInfo rigid_constraintChild;
            public static ChildInfo instance_physics_modelChild;
            public static ChildInfo extraChild;
        }

        public static class rigid_body
        {
            public static DomNodeType Type;
            public static AttributeInfo sidAttribute;
            public static AttributeInfo nameAttribute;
            public static ChildInfo technique_commonChild;
            public static ChildInfo techniqueChild;
            public static ChildInfo extraChild;
        }

        public static class rigid_body_technique_common
        {
            public static DomNodeType Type;
            public static ChildInfo dynamicChild;
            public static ChildInfo massChild;
            public static ChildInfo mass_frameChild;
            public static ChildInfo inertiaChild;
            public static ChildInfo instance_physics_materialChild;
            public static ChildInfo physics_materialChild;
            public static ChildInfo shapeChild;
        }

        public static class technique_common_dynamic
        {
            public static DomNodeType Type;
            public static AttributeInfo Attribute;
            public static AttributeInfo sidAttribute;
        }

        public static class technique_common_mass_frame
        {
            public static DomNodeType Type;
            public static ChildInfo translateChild;
            public static ChildInfo rotateChild;
        }

        public static class technique_common_shape
        {
            public static DomNodeType Type;
            public static ChildInfo hollowChild;
            public static ChildInfo massChild;
            public static ChildInfo densityChild;
            public static ChildInfo instance_physics_materialChild;
            public static ChildInfo physics_materialChild;
            public static ChildInfo instance_geometryChild;
            public static ChildInfo planeChild;
            public static ChildInfo boxChild;
            public static ChildInfo sphereChild;
            public static ChildInfo cylinderChild;
            public static ChildInfo tapered_cylinderChild;
            public static ChildInfo capsuleChild;
            public static ChildInfo tapered_capsuleChild;
            public static ChildInfo translateChild;
            public static ChildInfo rotateChild;
            public static ChildInfo extraChild;
        }

        public static class shape_hollow
        {
            public static DomNodeType Type;
            public static AttributeInfo Attribute;
            public static AttributeInfo sidAttribute;
        }

        public static class plane
        {
            public static DomNodeType Type;
            public static AttributeInfo equationAttribute;
            public static ChildInfo extraChild;
        }

        public static class box
        {
            public static DomNodeType Type;
            public static AttributeInfo half_extentsAttribute;
            public static ChildInfo extraChild;
        }

        public static class sphere
        {
            public static DomNodeType Type;
            public static AttributeInfo radiusAttribute;
            public static ChildInfo extraChild;
        }

        public static class cylinder
        {
            public static DomNodeType Type;
            public static AttributeInfo heightAttribute;
            public static AttributeInfo radiusAttribute;
            public static ChildInfo extraChild;
        }

        public static class tapered_cylinder
        {
            public static DomNodeType Type;
            public static AttributeInfo heightAttribute;
            public static AttributeInfo radius1Attribute;
            public static AttributeInfo radius2Attribute;
            public static ChildInfo extraChild;
        }

        public static class capsule
        {
            public static DomNodeType Type;
            public static AttributeInfo heightAttribute;
            public static AttributeInfo radiusAttribute;
            public static ChildInfo extraChild;
        }

        public static class tapered_capsule
        {
            public static DomNodeType Type;
            public static AttributeInfo heightAttribute;
            public static AttributeInfo radius1Attribute;
            public static AttributeInfo radius2Attribute;
            public static ChildInfo extraChild;
        }

        public static class rigid_constraint
        {
            public static DomNodeType Type;
            public static AttributeInfo sidAttribute;
            public static AttributeInfo nameAttribute;
            public static ChildInfo ref_attachmentChild;
            public static ChildInfo attachmentChild;
            public static ChildInfo technique_commonChild;
            public static ChildInfo techniqueChild;
            public static ChildInfo extraChild;
        }

        public static class rigid_constraint_ref_attachment
        {
            public static DomNodeType Type;
            public static AttributeInfo rigid_bodyAttribute;
            public static ChildInfo translateChild;
            public static ChildInfo rotateChild;
            public static ChildInfo extraChild;
        }

        public static class rigid_constraint_attachment
        {
            public static DomNodeType Type;
            public static AttributeInfo rigid_bodyAttribute;
            public static ChildInfo translateChild;
            public static ChildInfo rotateChild;
            public static ChildInfo extraChild;
        }

        public static class rigid_constraint_technique_common
        {
            public static DomNodeType Type;
            public static ChildInfo enabledChild;
            public static ChildInfo interpenetrateChild;
            public static ChildInfo limitsChild;
            public static ChildInfo springChild;
        }

        public static class technique_common_enabled
        {
            public static DomNodeType Type;
            public static AttributeInfo Attribute;
            public static AttributeInfo sidAttribute;
        }

        public static class technique_common_interpenetrate
        {
            public static DomNodeType Type;
            public static AttributeInfo Attribute;
            public static AttributeInfo sidAttribute;
        }

        public static class technique_common_limits
        {
            public static DomNodeType Type;
            public static ChildInfo swing_cone_and_twistChild;
            public static ChildInfo linearChild;
        }

        public static class limits_swing_cone_and_twist
        {
            public static DomNodeType Type;
            public static ChildInfo minChild;
            public static ChildInfo maxChild;
        }

        public static class limits_linear
        {
            public static DomNodeType Type;
            public static ChildInfo minChild;
            public static ChildInfo maxChild;
        }

        public static class technique_common_spring
        {
            public static DomNodeType Type;
            public static ChildInfo angularChild;
            public static ChildInfo linearChild;
        }

        public static class spring_angular
        {
            public static DomNodeType Type;
            public static ChildInfo stiffnessChild;
            public static ChildInfo dampingChild;
            public static ChildInfo target_valueChild;
        }

        public static class spring_linear
        {
            public static DomNodeType Type;
            public static ChildInfo stiffnessChild;
            public static ChildInfo dampingChild;
            public static ChildInfo target_valueChild;
        }

        public static class instance_physics_model
        {
            public static DomNodeType Type;
            public static AttributeInfo urlAttribute;
            public static AttributeInfo sidAttribute;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo parentAttribute;
            public static ChildInfo instance_force_fieldChild;
            public static ChildInfo instance_rigid_bodyChild;
            public static ChildInfo instance_rigid_constraintChild;
            public static ChildInfo extraChild;
        }

        public static class instance_rigid_body
        {
            public static DomNodeType Type;
            public static AttributeInfo bodyAttribute;
            public static AttributeInfo sidAttribute;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo targetAttribute;
            public static ChildInfo technique_commonChild;
            public static ChildInfo techniqueChild;
            public static ChildInfo extraChild;
        }

        public static class instance_rigid_body_technique_common
        {
            public static DomNodeType Type;
            public static AttributeInfo angular_velocityAttribute;
            public static AttributeInfo velocityAttribute;
            public static ChildInfo dynamicChild;
            public static ChildInfo massChild;
            public static ChildInfo mass_frameChild;
            public static ChildInfo inertiaChild;
            public static ChildInfo instance_physics_materialChild;
            public static ChildInfo physics_materialChild;
            public static ChildInfo shapeChild;
        }

        public static class instance_rigid_body_technique_common_dynamic
        {
            public static DomNodeType Type;
            public static AttributeInfo Attribute;
            public static AttributeInfo sidAttribute;
        }

        public static class instance_rigid_body_technique_common_mass_frame
        {
            public static DomNodeType Type;
            public static ChildInfo translateChild;
            public static ChildInfo rotateChild;
        }

        public static class instance_rigid_body_technique_common_shape
        {
            public static DomNodeType Type;
            public static ChildInfo hollowChild;
            public static ChildInfo massChild;
            public static ChildInfo densityChild;
            public static ChildInfo instance_physics_materialChild;
            public static ChildInfo physics_materialChild;
            public static ChildInfo instance_geometryChild;
            public static ChildInfo planeChild;
            public static ChildInfo boxChild;
            public static ChildInfo sphereChild;
            public static ChildInfo cylinderChild;
            public static ChildInfo tapered_cylinderChild;
            public static ChildInfo capsuleChild;
            public static ChildInfo tapered_capsuleChild;
            public static ChildInfo translateChild;
            public static ChildInfo rotateChild;
            public static ChildInfo extraChild;
        }

        public static class technique_common_shape_hollow
        {
            public static DomNodeType Type;
            public static AttributeInfo Attribute;
            public static AttributeInfo sidAttribute;
        }

        public static class instance_rigid_constraint
        {
            public static DomNodeType Type;
            public static AttributeInfo constraintAttribute;
            public static AttributeInfo sidAttribute;
            public static AttributeInfo nameAttribute;
            public static ChildInfo extraChild;
        }

        public static class library_physics_scenes
        {
            public static DomNodeType Type;
            public static AttributeInfo idAttribute;
            public static AttributeInfo nameAttribute;
            public static ChildInfo assetChild;
            public static ChildInfo physics_sceneChild;
            public static ChildInfo extraChild;
        }

        public static class physics_scene
        {
            public static DomNodeType Type;
            public static AttributeInfo idAttribute;
            public static AttributeInfo nameAttribute;
            public static ChildInfo assetChild;
            public static ChildInfo instance_force_fieldChild;
            public static ChildInfo instance_physics_modelChild;
            public static ChildInfo technique_commonChild;
            public static ChildInfo techniqueChild;
            public static ChildInfo extraChild;
        }

        public static class physics_scene_technique_common
        {
            public static DomNodeType Type;
            public static ChildInfo gravityChild;
            public static ChildInfo time_stepChild;
        }

        public static class library_visual_scenes
        {
            public static DomNodeType Type;
            public static AttributeInfo idAttribute;
            public static AttributeInfo nameAttribute;
            public static ChildInfo assetChild;
            public static ChildInfo visual_sceneChild;
            public static ChildInfo extraChild;
        }

        public static class visual_scene
        {
            public static DomNodeType Type;
            public static AttributeInfo idAttribute;
            public static AttributeInfo nameAttribute;
            public static ChildInfo assetChild;
            public static ChildInfo nodeChild;
            public static ChildInfo evaluate_sceneChild;
            public static ChildInfo extraChild;
        }

        public static class visual_scene_evaluate_scene
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static ChildInfo renderChild;
        }

        public static class evaluate_scene_render
        {
            public static DomNodeType Type;
            public static AttributeInfo camera_nodeAttribute;
            public static ChildInfo layerChild;
            public static ChildInfo instance_effectChild;
        }

        public static class COLLADA_scene
        {
            public static DomNodeType Type;
            public static ChildInfo instance_physics_sceneChild;
            public static ChildInfo instance_visual_sceneChild;
            public static ChildInfo extraChild;
        }

        public static class profile_GLSL
        {
            public static DomNodeType Type;
            public static AttributeInfo idAttribute;
            public static ChildInfo assetChild;
            public static ChildInfo codeChild;
            public static ChildInfo includeChild;
            public static ChildInfo imageChild;
            public static ChildInfo newparamChild;
            public static ChildInfo techniqueChild;
            public static ChildInfo extraChild;
        }

        public static class glsl_newparam
        {
            public static DomNodeType Type;
            public static AttributeInfo semanticAttribute;
            public static AttributeInfo modifierAttribute;
            public static AttributeInfo boolAttribute;
            public static AttributeInfo bool2Attribute;
            public static AttributeInfo bool3Attribute;
            public static AttributeInfo bool4Attribute;
            public static AttributeInfo floatAttribute;
            public static AttributeInfo float2Attribute;
            public static AttributeInfo float3Attribute;
            public static AttributeInfo float4Attribute;
            public static AttributeInfo float2x2Attribute;
            public static AttributeInfo float3x3Attribute;
            public static AttributeInfo float4x4Attribute;
            public static AttributeInfo intAttribute;
            public static AttributeInfo int2Attribute;
            public static AttributeInfo int3Attribute;
            public static AttributeInfo int4Attribute;
            public static AttributeInfo enumAttribute;
            public static AttributeInfo sidAttribute;
            public static ChildInfo annotateChild;
            public static ChildInfo surfaceChild;
            public static ChildInfo sampler1DChild;
            public static ChildInfo sampler2DChild;
            public static ChildInfo sampler3DChild;
            public static ChildInfo samplerCUBEChild;
            public static ChildInfo samplerRECTChild;
            public static ChildInfo samplerDEPTHChild;
            public static ChildInfo arrayChild;
        }

        public static class glsl_surface_type
        {
            public static DomNodeType Type;
            public static AttributeInfo formatAttribute;
            public static AttributeInfo sizeAttribute;
            public static AttributeInfo viewport_ratioAttribute;
            public static AttributeInfo mip_levelsAttribute;
            public static AttributeInfo mipmap_generateAttribute;
            public static AttributeInfo typeAttribute;
            public static ChildInfo init_as_nullChild;
            public static ChildInfo init_as_targetChild;
            public static ChildInfo init_cubeChild;
            public static ChildInfo init_volumeChild;
            public static ChildInfo init_planarChild;
            public static ChildInfo init_fromChild;
            public static ChildInfo format_hintChild;
            public static ChildInfo extraChild;
            public static ChildInfo generatorChild;
        }

        public static class glsl_surface_type_generator
        {
            public static DomNodeType Type;
            public static ChildInfo annotateChild;
            public static ChildInfo codeChild;
            public static ChildInfo includeChild;
            public static ChildInfo nameChild;
            public static ChildInfo setparamChild;
        }

        public static class glsl_surface_type_generator_name
        {
            public static DomNodeType Type;
            public static AttributeInfo Attribute;
            public static AttributeInfo sourceAttribute;
        }

        public static class glsl_setparam_simple
        {
            public static DomNodeType Type;
            public static AttributeInfo boolAttribute;
            public static AttributeInfo bool2Attribute;
            public static AttributeInfo bool3Attribute;
            public static AttributeInfo bool4Attribute;
            public static AttributeInfo floatAttribute;
            public static AttributeInfo float2Attribute;
            public static AttributeInfo float3Attribute;
            public static AttributeInfo float4Attribute;
            public static AttributeInfo float2x2Attribute;
            public static AttributeInfo float3x3Attribute;
            public static AttributeInfo float4x4Attribute;
            public static AttributeInfo intAttribute;
            public static AttributeInfo int2Attribute;
            public static AttributeInfo int3Attribute;
            public static AttributeInfo int4Attribute;
            public static AttributeInfo enumAttribute;
            public static AttributeInfo refAttribute;
            public static ChildInfo annotateChild;
            public static ChildInfo surfaceChild;
            public static ChildInfo sampler1DChild;
            public static ChildInfo sampler2DChild;
            public static ChildInfo sampler3DChild;
            public static ChildInfo samplerCUBEChild;
            public static ChildInfo samplerRECTChild;
            public static ChildInfo samplerDEPTHChild;
        }

        public static class glsl_newarray_type
        {
            public static DomNodeType Type;
            public static AttributeInfo boolAttribute;
            public static AttributeInfo bool2Attribute;
            public static AttributeInfo bool3Attribute;
            public static AttributeInfo bool4Attribute;
            public static AttributeInfo floatAttribute;
            public static AttributeInfo float2Attribute;
            public static AttributeInfo float3Attribute;
            public static AttributeInfo float4Attribute;
            public static AttributeInfo float2x2Attribute;
            public static AttributeInfo float3x3Attribute;
            public static AttributeInfo float4x4Attribute;
            public static AttributeInfo intAttribute;
            public static AttributeInfo int2Attribute;
            public static AttributeInfo int3Attribute;
            public static AttributeInfo int4Attribute;
            public static AttributeInfo enumAttribute;
            public static AttributeInfo lengthAttribute;
            public static ChildInfo surfaceChild;
            public static ChildInfo sampler1DChild;
            public static ChildInfo sampler2DChild;
            public static ChildInfo sampler3DChild;
            public static ChildInfo samplerCUBEChild;
            public static ChildInfo samplerRECTChild;
            public static ChildInfo samplerDEPTHChild;
            public static ChildInfo arrayChild;
        }

        public static class profile_GLSL_technique
        {
            public static DomNodeType Type;
            public static AttributeInfo idAttribute;
            public static AttributeInfo sidAttribute;
            public static ChildInfo annotateChild;
            public static ChildInfo codeChild;
            public static ChildInfo includeChild;
            public static ChildInfo imageChild;
            public static ChildInfo newparamChild;
            public static ChildInfo setparamChild;
            public static ChildInfo passChild;
            public static ChildInfo extraChild;
        }

        public static class glsl_setparam
        {
            public static DomNodeType Type;
            public static AttributeInfo boolAttribute;
            public static AttributeInfo bool2Attribute;
            public static AttributeInfo bool3Attribute;
            public static AttributeInfo bool4Attribute;
            public static AttributeInfo floatAttribute;
            public static AttributeInfo float2Attribute;
            public static AttributeInfo float3Attribute;
            public static AttributeInfo float4Attribute;
            public static AttributeInfo float2x2Attribute;
            public static AttributeInfo float3x3Attribute;
            public static AttributeInfo float4x4Attribute;
            public static AttributeInfo intAttribute;
            public static AttributeInfo int2Attribute;
            public static AttributeInfo int3Attribute;
            public static AttributeInfo int4Attribute;
            public static AttributeInfo enumAttribute;
            public static AttributeInfo refAttribute;
            public static AttributeInfo programAttribute;
            public static ChildInfo annotateChild;
            public static ChildInfo surfaceChild;
            public static ChildInfo sampler1DChild;
            public static ChildInfo sampler2DChild;
            public static ChildInfo sampler3DChild;
            public static ChildInfo samplerCUBEChild;
            public static ChildInfo samplerRECTChild;
            public static ChildInfo samplerDEPTHChild;
            public static ChildInfo arrayChild;
        }

        public static class glsl_setarray_type
        {
            public static DomNodeType Type;
            public static AttributeInfo boolAttribute;
            public static AttributeInfo bool2Attribute;
            public static AttributeInfo bool3Attribute;
            public static AttributeInfo bool4Attribute;
            public static AttributeInfo floatAttribute;
            public static AttributeInfo float2Attribute;
            public static AttributeInfo float3Attribute;
            public static AttributeInfo float4Attribute;
            public static AttributeInfo float2x2Attribute;
            public static AttributeInfo float3x3Attribute;
            public static AttributeInfo float4x4Attribute;
            public static AttributeInfo intAttribute;
            public static AttributeInfo int2Attribute;
            public static AttributeInfo int3Attribute;
            public static AttributeInfo int4Attribute;
            public static AttributeInfo enumAttribute;
            public static AttributeInfo lengthAttribute;
            public static ChildInfo surfaceChild;
            public static ChildInfo sampler1DChild;
            public static ChildInfo sampler2DChild;
            public static ChildInfo sampler3DChild;
            public static ChildInfo samplerCUBEChild;
            public static ChildInfo samplerRECTChild;
            public static ChildInfo samplerDEPTHChild;
            public static ChildInfo arrayChild;
        }

        public static class profile_GLSL_technique_pass
        {
            public static DomNodeType Type;
            public static AttributeInfo drawAttribute;
            public static AttributeInfo sidAttribute;
            public static ChildInfo annotateChild;
            public static ChildInfo color_targetChild;
            public static ChildInfo depth_targetChild;
            public static ChildInfo stencil_targetChild;
            public static ChildInfo color_clearChild;
            public static ChildInfo depth_clearChild;
            public static ChildInfo stencil_clearChild;
            public static ChildInfo alpha_funcChild;
            public static ChildInfo blend_funcChild;
            public static ChildInfo blend_func_separateChild;
            public static ChildInfo blend_equationChild;
            public static ChildInfo blend_equation_separateChild;
            public static ChildInfo color_materialChild;
            public static ChildInfo cull_faceChild;
            public static ChildInfo depth_funcChild;
            public static ChildInfo fog_modeChild;
            public static ChildInfo fog_coord_srcChild;
            public static ChildInfo front_faceChild;
            public static ChildInfo light_model_color_controlChild;
            public static ChildInfo logic_opChild;
            public static ChildInfo polygon_modeChild;
            public static ChildInfo shade_modelChild;
            public static ChildInfo stencil_funcChild;
            public static ChildInfo stencil_opChild;
            public static ChildInfo stencil_func_separateChild;
            public static ChildInfo stencil_op_separateChild;
            public static ChildInfo stencil_mask_separateChild;
            public static ChildInfo light_enableChild;
            public static ChildInfo light_ambientChild;
            public static ChildInfo light_diffuseChild;
            public static ChildInfo light_specularChild;
            public static ChildInfo light_positionChild;
            public static ChildInfo light_constant_attenuationChild;
            public static ChildInfo light_linear_attenuationChild;
            public static ChildInfo light_quadratic_attenuationChild;
            public static ChildInfo light_spot_cutoffChild;
            public static ChildInfo light_spot_directionChild;
            public static ChildInfo light_spot_exponentChild;
            public static ChildInfo texture1DChild;
            public static ChildInfo texture2DChild;
            public static ChildInfo texture3DChild;
            public static ChildInfo textureCUBEChild;
            public static ChildInfo textureRECTChild;
            public static ChildInfo textureDEPTHChild;
            public static ChildInfo texture1D_enableChild;
            public static ChildInfo texture2D_enableChild;
            public static ChildInfo texture3D_enableChild;
            public static ChildInfo textureCUBE_enableChild;
            public static ChildInfo textureRECT_enableChild;
            public static ChildInfo textureDEPTH_enableChild;
            public static ChildInfo texture_env_colorChild;
            public static ChildInfo texture_env_modeChild;
            public static ChildInfo clip_planeChild;
            public static ChildInfo clip_plane_enableChild;
            public static ChildInfo blend_colorChild;
            public static ChildInfo clear_colorChild;
            public static ChildInfo clear_stencilChild;
            public static ChildInfo clear_depthChild;
            public static ChildInfo color_maskChild;
            public static ChildInfo depth_boundsChild;
            public static ChildInfo depth_maskChild;
            public static ChildInfo depth_rangeChild;
            public static ChildInfo fog_densityChild;
            public static ChildInfo fog_startChild;
            public static ChildInfo fog_endChild;
            public static ChildInfo fog_colorChild;
            public static ChildInfo light_model_ambientChild;
            public static ChildInfo lighting_enableChild;
            public static ChildInfo line_stippleChild;
            public static ChildInfo line_widthChild;
            public static ChildInfo material_ambientChild;
            public static ChildInfo material_diffuseChild;
            public static ChildInfo material_emissionChild;
            public static ChildInfo material_shininessChild;
            public static ChildInfo material_specularChild;
            public static ChildInfo model_view_matrixChild;
            public static ChildInfo point_distance_attenuationChild;
            public static ChildInfo point_fade_threshold_sizeChild;
            public static ChildInfo point_sizeChild;
            public static ChildInfo point_size_minChild;
            public static ChildInfo point_size_maxChild;
            public static ChildInfo polygon_offsetChild;
            public static ChildInfo projection_matrixChild;
            public static ChildInfo scissorChild;
            public static ChildInfo stencil_maskChild;
            public static ChildInfo alpha_test_enableChild;
            public static ChildInfo auto_normal_enableChild;
            public static ChildInfo blend_enableChild;
            public static ChildInfo color_logic_op_enableChild;
            public static ChildInfo color_material_enableChild;
            public static ChildInfo cull_face_enableChild;
            public static ChildInfo depth_bounds_enableChild;
            public static ChildInfo depth_clamp_enableChild;
            public static ChildInfo depth_test_enableChild;
            public static ChildInfo dither_enableChild;
            public static ChildInfo fog_enableChild;
            public static ChildInfo light_model_local_viewer_enableChild;
            public static ChildInfo light_model_two_side_enableChild;
            public static ChildInfo line_smooth_enableChild;
            public static ChildInfo line_stipple_enableChild;
            public static ChildInfo logic_op_enableChild;
            public static ChildInfo multisample_enableChild;
            public static ChildInfo normalize_enableChild;
            public static ChildInfo point_smooth_enableChild;
            public static ChildInfo polygon_offset_fill_enableChild;
            public static ChildInfo polygon_offset_line_enableChild;
            public static ChildInfo polygon_offset_point_enableChild;
            public static ChildInfo polygon_smooth_enableChild;
            public static ChildInfo polygon_stipple_enableChild;
            public static ChildInfo rescale_normal_enableChild;
            public static ChildInfo sample_alpha_to_coverage_enableChild;
            public static ChildInfo sample_alpha_to_one_enableChild;
            public static ChildInfo sample_coverage_enableChild;
            public static ChildInfo scissor_test_enableChild;
            public static ChildInfo stencil_test_enableChild;
            public static ChildInfo gl_hook_abstractChild;
            public static ChildInfo shaderChild;
            public static ChildInfo extraChild;
        }

        public static class technique_pass_shader
        {
            public static DomNodeType Type;
            public static AttributeInfo compiler_optionsAttribute;
            public static AttributeInfo stageAttribute;
            public static ChildInfo annotateChild;
            public static ChildInfo compiler_targetChild;
            public static ChildInfo nameChild;
            public static ChildInfo bindChild;
        }

        public static class pass_shader_compiler_target
        {
            public static DomNodeType Type;
            public static AttributeInfo Attribute;
        }

        public static class pass_shader_name
        {
            public static DomNodeType Type;
            public static AttributeInfo Attribute;
            public static AttributeInfo sourceAttribute;
        }

        public static class pass_shader_bind
        {
            public static DomNodeType Type;
            public static AttributeInfo boolAttribute;
            public static AttributeInfo bool2Attribute;
            public static AttributeInfo bool3Attribute;
            public static AttributeInfo bool4Attribute;
            public static AttributeInfo floatAttribute;
            public static AttributeInfo float2Attribute;
            public static AttributeInfo float3Attribute;
            public static AttributeInfo float4Attribute;
            public static AttributeInfo float2x2Attribute;
            public static AttributeInfo float3x3Attribute;
            public static AttributeInfo float4x4Attribute;
            public static AttributeInfo intAttribute;
            public static AttributeInfo int2Attribute;
            public static AttributeInfo int3Attribute;
            public static AttributeInfo int4Attribute;
            public static AttributeInfo enumAttribute;
            public static AttributeInfo symbolAttribute;
            public static ChildInfo surfaceChild;
            public static ChildInfo sampler1DChild;
            public static ChildInfo sampler2DChild;
            public static ChildInfo sampler3DChild;
            public static ChildInfo samplerCUBEChild;
            public static ChildInfo samplerRECTChild;
            public static ChildInfo samplerDEPTHChild;
            public static ChildInfo paramChild;
        }

        public static class shader_bind_param
        {
            public static DomNodeType Type;
            public static AttributeInfo refAttribute;
        }

        public static class profile_GLES
        {
            public static DomNodeType Type;
            public static AttributeInfo idAttribute;
            public static AttributeInfo platformAttribute;
            public static ChildInfo assetChild;
            public static ChildInfo imageChild;
            public static ChildInfo newparamChild;
            public static ChildInfo techniqueChild;
            public static ChildInfo extraChild;
        }

        public static class gles_newparam
        {
            public static DomNodeType Type;
            public static AttributeInfo semanticAttribute;
            public static AttributeInfo modifierAttribute;
            public static AttributeInfo boolAttribute;
            public static AttributeInfo bool2Attribute;
            public static AttributeInfo bool3Attribute;
            public static AttributeInfo bool4Attribute;
            public static AttributeInfo intAttribute;
            public static AttributeInfo int2Attribute;
            public static AttributeInfo int3Attribute;
            public static AttributeInfo int4Attribute;
            public static AttributeInfo floatAttribute;
            public static AttributeInfo float2Attribute;
            public static AttributeInfo float3Attribute;
            public static AttributeInfo float4Attribute;
            public static AttributeInfo float1x1Attribute;
            public static AttributeInfo float1x2Attribute;
            public static AttributeInfo float1x3Attribute;
            public static AttributeInfo float1x4Attribute;
            public static AttributeInfo float2x1Attribute;
            public static AttributeInfo float2x2Attribute;
            public static AttributeInfo float2x3Attribute;
            public static AttributeInfo float2x4Attribute;
            public static AttributeInfo float3x1Attribute;
            public static AttributeInfo float3x2Attribute;
            public static AttributeInfo float3x3Attribute;
            public static AttributeInfo float3x4Attribute;
            public static AttributeInfo float4x1Attribute;
            public static AttributeInfo float4x2Attribute;
            public static AttributeInfo float4x3Attribute;
            public static AttributeInfo float4x4Attribute;
            public static AttributeInfo enumAttribute;
            public static AttributeInfo sidAttribute;
            public static ChildInfo annotateChild;
            public static ChildInfo surfaceChild;
            public static ChildInfo texture_pipelineChild;
            public static ChildInfo sampler_stateChild;
            public static ChildInfo texture_unitChild;
        }

        public static class gles_texture_pipeline
        {
            public static DomNodeType Type;
            public static AttributeInfo sidAttribute;
            public static ChildInfo texcombinerChild;
            public static ChildInfo texenvChild;
            public static ChildInfo extraChild;
        }

        public static class gles_texcombiner_command_type
        {
            public static DomNodeType Type;
            public static ChildInfo constantChild;
            public static ChildInfo RGBChild;
            public static ChildInfo alphaChild;
        }

        public static class gles_texture_constant_type
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class gles_texcombiner_commandRGB_type
        {
            public static DomNodeType Type;
            public static AttributeInfo operatorAttribute;
            public static AttributeInfo scaleAttribute;
            public static ChildInfo argumentChild;
        }

        public static class gles_texcombiner_argumentRGB_type
        {
            public static DomNodeType Type;
            public static AttributeInfo sourceAttribute;
            public static AttributeInfo operandAttribute;
            public static AttributeInfo unitAttribute;
        }

        public static class gles_texcombiner_commandAlpha_type
        {
            public static DomNodeType Type;
            public static AttributeInfo operatorAttribute;
            public static AttributeInfo scaleAttribute;
            public static ChildInfo argumentChild;
        }

        public static class gles_texcombiner_argumentAlpha_type
        {
            public static DomNodeType Type;
            public static AttributeInfo sourceAttribute;
            public static AttributeInfo operandAttribute;
            public static AttributeInfo unitAttribute;
        }

        public static class gles_texenv_command_type
        {
            public static DomNodeType Type;
            public static AttributeInfo operatorAttribute;
            public static AttributeInfo unitAttribute;
            public static ChildInfo constantChild;
        }

        public static class gles_sampler_state
        {
            public static DomNodeType Type;
            public static AttributeInfo wrap_sAttribute;
            public static AttributeInfo wrap_tAttribute;
            public static AttributeInfo minfilterAttribute;
            public static AttributeInfo magfilterAttribute;
            public static AttributeInfo mipfilterAttribute;
            public static AttributeInfo mipmap_maxlevelAttribute;
            public static AttributeInfo mipmap_biasAttribute;
            public static AttributeInfo sidAttribute;
            public static ChildInfo extraChild;
        }

        public static class gles_texture_unit
        {
            public static DomNodeType Type;
            public static AttributeInfo surfaceAttribute;
            public static AttributeInfo sampler_stateAttribute;
            public static AttributeInfo sidAttribute;
            public static ChildInfo texcoordChild;
            public static ChildInfo extraChild;
        }

        public static class gles_texture_unit_texcoord
        {
            public static DomNodeType Type;
            public static AttributeInfo semanticAttribute;
        }

        public static class profile_GLES_technique
        {
            public static DomNodeType Type;
            public static AttributeInfo idAttribute;
            public static AttributeInfo sidAttribute;
            public static ChildInfo assetChild;
            public static ChildInfo annotateChild;
            public static ChildInfo imageChild;
            public static ChildInfo newparamChild;
            public static ChildInfo setparamChild;
            public static ChildInfo passChild;
            public static ChildInfo extraChild;
        }

        public static class technique_setparam
        {
            public static DomNodeType Type;
            public static AttributeInfo boolAttribute;
            public static AttributeInfo bool2Attribute;
            public static AttributeInfo bool3Attribute;
            public static AttributeInfo bool4Attribute;
            public static AttributeInfo intAttribute;
            public static AttributeInfo int2Attribute;
            public static AttributeInfo int3Attribute;
            public static AttributeInfo int4Attribute;
            public static AttributeInfo floatAttribute;
            public static AttributeInfo float2Attribute;
            public static AttributeInfo float3Attribute;
            public static AttributeInfo float4Attribute;
            public static AttributeInfo float1x1Attribute;
            public static AttributeInfo float1x2Attribute;
            public static AttributeInfo float1x3Attribute;
            public static AttributeInfo float1x4Attribute;
            public static AttributeInfo float2x1Attribute;
            public static AttributeInfo float2x2Attribute;
            public static AttributeInfo float2x3Attribute;
            public static AttributeInfo float2x4Attribute;
            public static AttributeInfo float3x1Attribute;
            public static AttributeInfo float3x2Attribute;
            public static AttributeInfo float3x3Attribute;
            public static AttributeInfo float3x4Attribute;
            public static AttributeInfo float4x1Attribute;
            public static AttributeInfo float4x2Attribute;
            public static AttributeInfo float4x3Attribute;
            public static AttributeInfo float4x4Attribute;
            public static AttributeInfo enumAttribute;
            public static AttributeInfo refAttribute;
            public static ChildInfo annotateChild;
            public static ChildInfo surfaceChild;
            public static ChildInfo texture_pipelineChild;
            public static ChildInfo sampler_stateChild;
            public static ChildInfo texture_unitChild;
        }

        public static class profile_GLES_technique_pass
        {
            public static DomNodeType Type;
            public static AttributeInfo color_targetAttribute;
            public static AttributeInfo depth_targetAttribute;
            public static AttributeInfo stencil_targetAttribute;
            public static AttributeInfo color_clearAttribute;
            public static AttributeInfo depth_clearAttribute;
            public static AttributeInfo stencil_clearAttribute;
            public static AttributeInfo drawAttribute;
            public static AttributeInfo sidAttribute;
            public static ChildInfo annotateChild;
            public static ChildInfo alpha_funcChild;
            public static ChildInfo blend_funcChild;
            public static ChildInfo clear_colorChild;
            public static ChildInfo clear_stencilChild;
            public static ChildInfo clear_depthChild;
            public static ChildInfo clip_planeChild;
            public static ChildInfo color_maskChild;
            public static ChildInfo cull_faceChild;
            public static ChildInfo depth_funcChild;
            public static ChildInfo depth_maskChild;
            public static ChildInfo depth_rangeChild;
            public static ChildInfo fog_colorChild;
            public static ChildInfo fog_densityChild;
            public static ChildInfo fog_modeChild;
            public static ChildInfo fog_startChild;
            public static ChildInfo fog_endChild;
            public static ChildInfo front_faceChild;
            public static ChildInfo texture_pipelineChild;
            public static ChildInfo logic_opChild;
            public static ChildInfo light_ambientChild;
            public static ChildInfo light_diffuseChild;
            public static ChildInfo light_specularChild;
            public static ChildInfo light_positionChild;
            public static ChildInfo light_constant_attenuationChild;
            public static ChildInfo light_linear_attenutationChild;
            public static ChildInfo light_quadratic_attenuationChild;
            public static ChildInfo light_spot_cutoffChild;
            public static ChildInfo light_spot_directionChild;
            public static ChildInfo light_spot_exponentChild;
            public static ChildInfo light_model_ambientChild;
            public static ChildInfo line_widthChild;
            public static ChildInfo material_ambientChild;
            public static ChildInfo material_diffuseChild;
            public static ChildInfo material_emissionChild;
            public static ChildInfo material_shininessChild;
            public static ChildInfo material_specularChild;
            public static ChildInfo model_view_matrixChild;
            public static ChildInfo point_distance_attenuationChild;
            public static ChildInfo point_fade_threshold_sizeChild;
            public static ChildInfo point_sizeChild;
            public static ChildInfo point_size_minChild;
            public static ChildInfo point_size_maxChild;
            public static ChildInfo polygon_offsetChild;
            public static ChildInfo projection_matrixChild;
            public static ChildInfo scissorChild;
            public static ChildInfo shade_modelChild;
            public static ChildInfo stencil_funcChild;
            public static ChildInfo stencil_maskChild;
            public static ChildInfo stencil_opChild;
            public static ChildInfo alpha_test_enableChild;
            public static ChildInfo blend_enableChild;
            public static ChildInfo clip_plane_enableChild;
            public static ChildInfo color_logic_op_enableChild;
            public static ChildInfo color_material_enableChild;
            public static ChildInfo cull_face_enableChild;
            public static ChildInfo depth_test_enableChild;
            public static ChildInfo dither_enableChild;
            public static ChildInfo fog_enableChild;
            public static ChildInfo texture_pipeline_enableChild;
            public static ChildInfo light_enableChild;
            public static ChildInfo lighting_enableChild;
            public static ChildInfo light_model_two_side_enableChild;
            public static ChildInfo line_smooth_enableChild;
            public static ChildInfo multisample_enableChild;
            public static ChildInfo normalize_enableChild;
            public static ChildInfo point_smooth_enableChild;
            public static ChildInfo polygon_offset_fill_enableChild;
            public static ChildInfo rescale_normal_enableChild;
            public static ChildInfo sample_alpha_to_coverage_enableChild;
            public static ChildInfo sample_alpha_to_one_enableChild;
            public static ChildInfo sample_coverage_enableChild;
            public static ChildInfo scissor_test_enableChild;
            public static ChildInfo stencil_test_enableChild;
            public static ChildInfo extraChild;
        }

        public static class texture_pipeline
        {
            public static DomNodeType Type;
            public static AttributeInfo paramAttribute;
            public static ChildInfo valueChild;
        }

        public static class light_linear_attenutation
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
            public static AttributeInfo indexAttribute;
        }

        public static class texture_pipeline_enable
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo paramAttribute;
        }

        public static class ellipsoid
        {
            public static DomNodeType Type;
            public static AttributeInfo sizeAttribute;
        }

        public static class InputGlobal
        {
            public static DomNodeType Type;
            public static AttributeInfo semanticAttribute;
            public static AttributeInfo sourceAttribute;
        }

        public static ChildInfo COLLADARootElement;

        public static ChildInfo IDREF_arrayRootElement;

        public static ChildInfo Name_arrayRootElement;

        public static ChildInfo bool_arrayRootElement;

        public static ChildInfo float_arrayRootElement;

        public static ChildInfo int_arrayRootElement;

        public static ChildInfo accessorRootElement;

        public static ChildInfo paramRootElement;

        public static ChildInfo sourceRootElement;

        public static ChildInfo geometryRootElement;

        public static ChildInfo meshRootElement;

        public static ChildInfo splineRootElement;

        public static ChildInfo pRootElement;

        public static ChildInfo linesRootElement;

        public static ChildInfo linestripsRootElement;

        public static ChildInfo polygonsRootElement;

        public static ChildInfo polylistRootElement;

        public static ChildInfo trianglesRootElement;

        public static ChildInfo trifansRootElement;

        public static ChildInfo tristripsRootElement;

        public static ChildInfo verticesRootElement;

        public static ChildInfo lookatRootElement;

        public static ChildInfo matrixRootElement;

        public static ChildInfo rotateRootElement;

        public static ChildInfo scaleRootElement;

        public static ChildInfo skewRootElement;

        public static ChildInfo translateRootElement;

        public static ChildInfo imageRootElement;

        public static ChildInfo lightRootElement;

        public static ChildInfo materialRootElement;

        public static ChildInfo cameraRootElement;

        public static ChildInfo animationRootElement;

        public static ChildInfo animation_clipRootElement;

        public static ChildInfo channelRootElement;

        public static ChildInfo samplerRootElement;

        public static ChildInfo controllerRootElement;

        public static ChildInfo skinRootElement;

        public static ChildInfo morphRootElement;

        public static ChildInfo assetRootElement;

        public static ChildInfo extraRootElement;

        public static ChildInfo techniqueRootElement;

        public static ChildInfo nodeRootElement;

        public static ChildInfo visual_sceneRootElement;

        public static ChildInfo bind_materialRootElement;

        public static ChildInfo instance_cameraRootElement;

        public static ChildInfo instance_controllerRootElement;

        public static ChildInfo instance_effectRootElement;

        public static ChildInfo instance_force_fieldRootElement;

        public static ChildInfo instance_geometryRootElement;

        public static ChildInfo instance_lightRootElement;

        public static ChildInfo instance_materialRootElement;

        public static ChildInfo instance_nodeRootElement;

        public static ChildInfo instance_physics_materialRootElement;

        public static ChildInfo instance_physics_modelRootElement;

        public static ChildInfo instance_rigid_bodyRootElement;

        public static ChildInfo instance_rigid_constraintRootElement;

        public static ChildInfo library_animationsRootElement;

        public static ChildInfo library_animation_clipsRootElement;

        public static ChildInfo library_camerasRootElement;

        public static ChildInfo library_controllersRootElement;

        public static ChildInfo library_geometriesRootElement;

        public static ChildInfo library_effectsRootElement;

        public static ChildInfo library_force_fieldsRootElement;

        public static ChildInfo library_imagesRootElement;

        public static ChildInfo library_lightsRootElement;

        public static ChildInfo library_materialsRootElement;

        public static ChildInfo library_nodesRootElement;

        public static ChildInfo library_physics_materialsRootElement;

        public static ChildInfo library_physics_modelsRootElement;

        public static ChildInfo library_physics_scenesRootElement;

        public static ChildInfo library_visual_scenesRootElement;

        public static ChildInfo fx_profile_abstractRootElement;

        public static ChildInfo effectRootElement;

        public static ChildInfo gl_hook_abstractRootElement;

        public static ChildInfo profile_GLSLRootElement;

        public static ChildInfo profile_COMMONRootElement;

        public static ChildInfo profile_CGRootElement;

        public static ChildInfo profile_GLESRootElement;

        public static ChildInfo boxRootElement;

        public static ChildInfo planeRootElement;

        public static ChildInfo sphereRootElement;

        public static ChildInfo ellipsoidRootElement;

        public static ChildInfo cylinderRootElement;

        public static ChildInfo tapered_cylinderRootElement;

        public static ChildInfo capsuleRootElement;

        public static ChildInfo tapered_capsuleRootElement;

        public static ChildInfo convex_meshRootElement;

        public static ChildInfo force_fieldRootElement;

        public static ChildInfo physics_materialRootElement;

        public static ChildInfo physics_sceneRootElement;

        public static ChildInfo rigid_bodyRootElement;

        public static ChildInfo rigid_constraintRootElement;

        public static ChildInfo physics_modelRootElement;
    }
}
