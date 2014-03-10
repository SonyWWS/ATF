// -------------------------------------------------------------------------------------------------------------------
// Generated code, do not edit
// Command Line:  DomGen "atgi.xsd" "Schema.cs" "http://www.atg.development.scee.net/atgi/1_29_0/atgi" "Sce.Atf.Atgi"
// -------------------------------------------------------------------------------------------------------------------

using Sce.Atf.Dom;

namespace Sce.Atf.Atgi
{
    public static class Schema
    {
        public const string NS = "http://www.atg.development.scee.net/atgi/1_29_0/atgi";

        public static void Initialize(XmlSchemaTypeCollection typeCollection)
        {
            worldType.Type = typeCollection.GetNodeType("worldType");
            worldType.nameAttribute = worldType.Type.GetAttributeInfo("name");
            worldType.filenameformAttribute = worldType.Type.GetAttributeInfo("filenameform");
            worldType.upaxisAttribute = worldType.Type.GetAttributeInfo("upaxis");
            worldType.parseaccelerationChild = worldType.Type.GetChildInfo("parseacceleration");
            worldType.animChannelChild = worldType.Type.GetChildInfo("animChannel");
            worldType.animDiscontinuitiesChild = worldType.Type.GetChildInfo("animDiscontinuities");
            worldType.animChild = worldType.Type.GetChildInfo("anim");
            worldType.aimConstraintChild = worldType.Type.GetChildInfo("aimConstraint");
            worldType.atgilocatorChild = worldType.Type.GetChildInfo("atgilocator");
            worldType.blendtargetChild = worldType.Type.GetChildInfo("blendtarget");
            worldType.cameraChild = worldType.Type.GetChildInfo("camera");
            worldType.constraintChild = worldType.Type.GetChildInfo("constraint");
            worldType.clusterChild = worldType.Type.GetChildInfo("cluster");
            worldType.dynamicTypeChild = worldType.Type.GetChildInfo("dynamicType");
            worldType.instanceChild = worldType.Type.GetChildInfo("instance");
            worldType.jointChild = worldType.Type.GetChildInfo("joint");
            worldType.lightChild = worldType.Type.GetChildInfo("light");
            worldType.locatorChild = worldType.Type.GetChildInfo("locator");
            worldType.lodgroupChild = worldType.Type.GetChildInfo("lodgroup");
            worldType.meshChild = worldType.Type.GetChildInfo("mesh");
            worldType.multiBlendTargetChild = worldType.Type.GetChildInfo("multiBlendTarget");
            worldType.nodeChild = worldType.Type.GetChildInfo("node");
            worldType.nurbsCurveChild = worldType.Type.GetChildInfo("nurbsCurve");
            worldType.nurbsChild = worldType.Type.GetChildInfo("nurbs");
            worldType.orientationConstraintChild = worldType.Type.GetChildInfo("orientationConstraint");
            worldType.parentConstraintChild = worldType.Type.GetChildInfo("parentConstraint");
            worldType.primitiveChild = worldType.Type.GetChildInfo("primitive");
            worldType.referenceChild = worldType.Type.GetChildInfo("reference");
            worldType.rigidBodyChild = worldType.Type.GetChildInfo("rigidBody");
            worldType.scaleConstraintChild = worldType.Type.GetChildInfo("scaleConstraint");
            worldType.springConstraintChild = worldType.Type.GetChildInfo("springConstraint");
            worldType.translationConstraintChild = worldType.Type.GetChildInfo("translationConstraint");
            worldType.animclipChild = worldType.Type.GetChildInfo("animclip");
            worldType.blendChild = worldType.Type.GetChildInfo("blend");
            worldType.blendshapeControllerChild = worldType.Type.GetChildInfo("blendshapeController");
            worldType.cgshaderChild = worldType.Type.GetChildInfo("cgshader");
            worldType.deformerChild = worldType.Type.GetChildInfo("deformer");
            worldType.expressionChild = worldType.Type.GetChildInfo("expression");
            worldType.imageChild = worldType.Type.GetChildInfo("image");
            worldType.materialChild = worldType.Type.GetChildInfo("material");
            worldType.motionPathChild = worldType.Type.GetChildInfo("motionPath");
            worldType.objsetChild = worldType.Type.GetChildInfo("objset");
            worldType.pmdataATGChild = worldType.Type.GetChildInfo("pmdataATG");
            worldType.poseChild = worldType.Type.GetChildInfo("pose");
            worldType.sceneChild = worldType.Type.GetChildInfo("scene");
            worldType.shaderChild = worldType.Type.GetChildInfo("shader");
            worldType.textureChild = worldType.Type.GetChildInfo("texture");
            worldType.blendshapeChild = worldType.Type.GetChildInfo("blendshape");
            worldType.skinChild = worldType.Type.GetChildInfo("skin");
            worldType.customDataChild = worldType.Type.GetChildInfo("customData");

            objType.Type = typeCollection.GetNodeType("objType");
            objType.nameAttribute = objType.Type.GetAttributeInfo("name");

            parseaccelerationType.Type = typeCollection.GetNodeType("parseaccelerationType");
            parseaccelerationType.filereferencesChild = parseaccelerationType.Type.GetChildInfo("filereferences");

            filereferencesType.Type = typeCollection.GetNodeType("filereferencesType");
            filereferencesType.fileChild = filereferencesType.Type.GetChildInfo("file");

            fileType.Type = typeCollection.GetNodeType("fileType");
            fileType.uriAttribute = fileType.Type.GetAttributeInfo("uri");

            animChannelType.Type = typeCollection.GetNodeType("animChannelType");
            animChannelType.nameAttribute = animChannelType.Type.GetAttributeInfo("name");
            animChannelType.channelAttribute = animChannelType.Type.GetAttributeInfo("channel");
            animChannelType.inputObjectAttribute = animChannelType.Type.GetAttributeInfo("inputObject");
            animChannelType.inputChannelAttribute = animChannelType.Type.GetAttributeInfo("inputChannel");
            animChannelType.animDataChild = animChannelType.Type.GetChildInfo("animData");

            animChannelType_animData.Type = typeCollection.GetNodeType("animChannelType_animData");
            animChannelType_animData.keyValuesAttribute = animChannelType_animData.Type.GetAttributeInfo("keyValues");
            animChannelType_animData.keyTimesAttribute = animChannelType_animData.Type.GetAttributeInfo("keyTimes");
            animChannelType_animData.tangentsAttribute = animChannelType_animData.Type.GetAttributeInfo("tangents");
            animChannelType_animData.interpAttribute = animChannelType_animData.Type.GetAttributeInfo("interp");
            animChannelType_animData.numKeysAttribute = animChannelType_animData.Type.GetAttributeInfo("numKeys");
            animChannelType_animData.keyStrideAttribute = animChannelType_animData.Type.GetAttributeInfo("keyStride");
            animChannelType_animData.timeOffsetAttribute = animChannelType_animData.Type.GetAttributeInfo("timeOffset");
            animChannelType_animData.durationAttribute = animChannelType_animData.Type.GetAttributeInfo("duration");

            animDiscontinuitiesType.Type = typeCollection.GetNodeType("animDiscontinuitiesType");
            animDiscontinuitiesType.nameAttribute = animDiscontinuitiesType.Type.GetAttributeInfo("name");
            animDiscontinuitiesType.keyStrideAttribute = animDiscontinuitiesType.Type.GetAttributeInfo("keyStride");
            animDiscontinuitiesType.cornerChild = animDiscontinuitiesType.Type.GetChildInfo("corner");
            animDiscontinuitiesType.stepChild = animDiscontinuitiesType.Type.GetChildInfo("step");

            animDiscontinuitiesType_corner.Type = typeCollection.GetNodeType("animDiscontinuitiesType_corner");
            animDiscontinuitiesType_corner.timeAttribute = animDiscontinuitiesType_corner.Type.GetAttributeInfo("time");

            animDiscontinuitiesType_step.Type = typeCollection.GetNodeType("animDiscontinuitiesType_step");
            animDiscontinuitiesType_step.beforeAttribute = animDiscontinuitiesType_step.Type.GetAttributeInfo("before");
            animDiscontinuitiesType_step.afterAttribute = animDiscontinuitiesType_step.Type.GetAttributeInfo("after");
            animDiscontinuitiesType_step.timeAttribute = animDiscontinuitiesType_step.Type.GetAttributeInfo("time");

            animType.Type = typeCollection.GetNodeType("animType");
            animType.nameAttribute = animType.Type.GetAttributeInfo("name");
            animType.targetAttribute = animType.Type.GetAttributeInfo("target");
            animType.animChannelChild = animType.Type.GetChildInfo("animChannel");
            animType.animDiscontinuitiesChild = animType.Type.GetChildInfo("animDiscontinuities");
            animType.animChild = animType.Type.GetChildInfo("anim");
            animType.aimConstraintChild = animType.Type.GetChildInfo("aimConstraint");
            animType.atgilocatorChild = animType.Type.GetChildInfo("atgilocator");
            animType.blendtargetChild = animType.Type.GetChildInfo("blendtarget");
            animType.cameraChild = animType.Type.GetChildInfo("camera");
            animType.constraintChild = animType.Type.GetChildInfo("constraint");
            animType.clusterChild = animType.Type.GetChildInfo("cluster");
            animType.dynamicTypeChild = animType.Type.GetChildInfo("dynamicType");
            animType.instanceChild = animType.Type.GetChildInfo("instance");
            animType.jointChild = animType.Type.GetChildInfo("joint");
            animType.lightChild = animType.Type.GetChildInfo("light");
            animType.locatorChild = animType.Type.GetChildInfo("locator");
            animType.lodgroupChild = animType.Type.GetChildInfo("lodgroup");
            animType.meshChild = animType.Type.GetChildInfo("mesh");
            animType.multiBlendTargetChild = animType.Type.GetChildInfo("multiBlendTarget");
            animType.nodeChild = animType.Type.GetChildInfo("node");
            animType.nurbsCurveChild = animType.Type.GetChildInfo("nurbsCurve");
            animType.nurbsChild = animType.Type.GetChildInfo("nurbs");
            animType.orientationConstraintChild = animType.Type.GetChildInfo("orientationConstraint");
            animType.parentConstraintChild = animType.Type.GetChildInfo("parentConstraint");
            animType.primitiveChild = animType.Type.GetChildInfo("primitive");
            animType.referenceChild = animType.Type.GetChildInfo("reference");
            animType.rigidBodyChild = animType.Type.GetChildInfo("rigidBody");
            animType.scaleConstraintChild = animType.Type.GetChildInfo("scaleConstraint");
            animType.springConstraintChild = animType.Type.GetChildInfo("springConstraint");
            animType.translationConstraintChild = animType.Type.GetChildInfo("translationConstraint");
            animType.animclipChild = animType.Type.GetChildInfo("animclip");
            animType.blendChild = animType.Type.GetChildInfo("blend");
            animType.blendshapeControllerChild = animType.Type.GetChildInfo("blendshapeController");
            animType.cgshaderChild = animType.Type.GetChildInfo("cgshader");
            animType.deformerChild = animType.Type.GetChildInfo("deformer");
            animType.expressionChild = animType.Type.GetChildInfo("expression");
            animType.imageChild = animType.Type.GetChildInfo("image");
            animType.materialChild = animType.Type.GetChildInfo("material");
            animType.motionPathChild = animType.Type.GetChildInfo("motionPath");
            animType.objsetChild = animType.Type.GetChildInfo("objset");
            animType.pmdataATGChild = animType.Type.GetChildInfo("pmdataATG");
            animType.poseChild = animType.Type.GetChildInfo("pose");
            animType.sceneChild = animType.Type.GetChildInfo("scene");
            animType.shaderChild = animType.Type.GetChildInfo("shader");
            animType.textureChild = animType.Type.GetChildInfo("texture");
            animType.blendshapeChild = animType.Type.GetChildInfo("blendshape");
            animType.skinChild = animType.Type.GetChildInfo("skin");
            animType.customDataChild = animType.Type.GetChildInfo("customData");

            aimConstraintType.Type = typeCollection.GetNodeType("aimConstraintType");
            aimConstraintType.nameAttribute = aimConstraintType.Type.GetAttributeInfo("name");
            aimConstraintType.aimAttribute = aimConstraintType.Type.GetAttributeInfo("aim");
            aimConstraintType.upAttribute = aimConstraintType.Type.GetAttributeInfo("up");
            aimConstraintType.globalupAttribute = aimConstraintType.Type.GetAttributeInfo("globalup");
            aimConstraintType.constrainAttribute = aimConstraintType.Type.GetAttributeInfo("constrain");
            aimConstraintType.offsetChild = aimConstraintType.Type.GetChildInfo("offset");
            aimConstraintType.upobjectChild = aimConstraintType.Type.GetChildInfo("upobject");
            aimConstraintType.targetChild = aimConstraintType.Type.GetChildInfo("target");
            aimConstraintType.animChannelChild = aimConstraintType.Type.GetChildInfo("animChannel");
            aimConstraintType.animDiscontinuitiesChild = aimConstraintType.Type.GetChildInfo("animDiscontinuities");
            aimConstraintType.animChild = aimConstraintType.Type.GetChildInfo("anim");
            aimConstraintType.aimConstraintChild = aimConstraintType.Type.GetChildInfo("aimConstraint");
            aimConstraintType.atgilocatorChild = aimConstraintType.Type.GetChildInfo("atgilocator");
            aimConstraintType.blendtargetChild = aimConstraintType.Type.GetChildInfo("blendtarget");
            aimConstraintType.cameraChild = aimConstraintType.Type.GetChildInfo("camera");
            aimConstraintType.constraintChild = aimConstraintType.Type.GetChildInfo("constraint");
            aimConstraintType.clusterChild = aimConstraintType.Type.GetChildInfo("cluster");
            aimConstraintType.dynamicTypeChild = aimConstraintType.Type.GetChildInfo("dynamicType");
            aimConstraintType.instanceChild = aimConstraintType.Type.GetChildInfo("instance");
            aimConstraintType.jointChild = aimConstraintType.Type.GetChildInfo("joint");
            aimConstraintType.lightChild = aimConstraintType.Type.GetChildInfo("light");
            aimConstraintType.locatorChild = aimConstraintType.Type.GetChildInfo("locator");
            aimConstraintType.lodgroupChild = aimConstraintType.Type.GetChildInfo("lodgroup");
            aimConstraintType.meshChild = aimConstraintType.Type.GetChildInfo("mesh");
            aimConstraintType.multiBlendTargetChild = aimConstraintType.Type.GetChildInfo("multiBlendTarget");
            aimConstraintType.nodeChild = aimConstraintType.Type.GetChildInfo("node");
            aimConstraintType.nurbsCurveChild = aimConstraintType.Type.GetChildInfo("nurbsCurve");
            aimConstraintType.nurbsChild = aimConstraintType.Type.GetChildInfo("nurbs");
            aimConstraintType.orientationConstraintChild = aimConstraintType.Type.GetChildInfo("orientationConstraint");
            aimConstraintType.parentConstraintChild = aimConstraintType.Type.GetChildInfo("parentConstraint");
            aimConstraintType.primitiveChild = aimConstraintType.Type.GetChildInfo("primitive");
            aimConstraintType.referenceChild = aimConstraintType.Type.GetChildInfo("reference");
            aimConstraintType.rigidBodyChild = aimConstraintType.Type.GetChildInfo("rigidBody");
            aimConstraintType.scaleConstraintChild = aimConstraintType.Type.GetChildInfo("scaleConstraint");
            aimConstraintType.springConstraintChild = aimConstraintType.Type.GetChildInfo("springConstraint");
            aimConstraintType.translationConstraintChild = aimConstraintType.Type.GetChildInfo("translationConstraint");
            aimConstraintType.animclipChild = aimConstraintType.Type.GetChildInfo("animclip");
            aimConstraintType.blendChild = aimConstraintType.Type.GetChildInfo("blend");
            aimConstraintType.blendshapeControllerChild = aimConstraintType.Type.GetChildInfo("blendshapeController");
            aimConstraintType.cgshaderChild = aimConstraintType.Type.GetChildInfo("cgshader");
            aimConstraintType.deformerChild = aimConstraintType.Type.GetChildInfo("deformer");
            aimConstraintType.expressionChild = aimConstraintType.Type.GetChildInfo("expression");
            aimConstraintType.imageChild = aimConstraintType.Type.GetChildInfo("image");
            aimConstraintType.materialChild = aimConstraintType.Type.GetChildInfo("material");
            aimConstraintType.motionPathChild = aimConstraintType.Type.GetChildInfo("motionPath");
            aimConstraintType.objsetChild = aimConstraintType.Type.GetChildInfo("objset");
            aimConstraintType.pmdataATGChild = aimConstraintType.Type.GetChildInfo("pmdataATG");
            aimConstraintType.poseChild = aimConstraintType.Type.GetChildInfo("pose");
            aimConstraintType.sceneChild = aimConstraintType.Type.GetChildInfo("scene");
            aimConstraintType.shaderChild = aimConstraintType.Type.GetChildInfo("shader");
            aimConstraintType.textureChild = aimConstraintType.Type.GetChildInfo("texture");
            aimConstraintType.blendshapeChild = aimConstraintType.Type.GetChildInfo("blendshape");
            aimConstraintType.skinChild = aimConstraintType.Type.GetChildInfo("skin");
            aimConstraintType.customDataChild = aimConstraintType.Type.GetChildInfo("customData");

            rotationType.Type = typeCollection.GetNodeType("rotationType");
            rotationType.Attribute = rotationType.Type.GetAttributeInfo("");
            rotationType.rotOrdAttribute = rotationType.Type.GetAttributeInfo("rotOrd");

            aimConstraintType_upobject.Type = typeCollection.GetNodeType("aimConstraintType_upobject");
            aimConstraintType_upobject.nameAttribute = aimConstraintType_upobject.Type.GetAttributeInfo("name");
            aimConstraintType_upobject.transformAttribute = aimConstraintType_upobject.Type.GetAttributeInfo("transform");

            constraintTargetType.Type = typeCollection.GetNodeType("constraintTargetType");
            constraintTargetType.nameAttribute = constraintTargetType.Type.GetAttributeInfo("name");
            constraintTargetType.weightAttribute = constraintTargetType.Type.GetAttributeInfo("weight");

            atgiLocatorType.Type = typeCollection.GetNodeType("atgiLocatorType");
            atgiLocatorType.nameAttribute = atgiLocatorType.Type.GetAttributeInfo("name");
            atgiLocatorType.localPositionAttribute = atgiLocatorType.Type.GetAttributeInfo("localPosition");
            atgiLocatorType.fileChild = atgiLocatorType.Type.GetChildInfo("file");
            atgiLocatorType.animChannelChild = atgiLocatorType.Type.GetChildInfo("animChannel");
            atgiLocatorType.animDiscontinuitiesChild = atgiLocatorType.Type.GetChildInfo("animDiscontinuities");
            atgiLocatorType.animChild = atgiLocatorType.Type.GetChildInfo("anim");
            atgiLocatorType.aimConstraintChild = atgiLocatorType.Type.GetChildInfo("aimConstraint");
            atgiLocatorType.atgilocatorChild = atgiLocatorType.Type.GetChildInfo("atgilocator");
            atgiLocatorType.blendtargetChild = atgiLocatorType.Type.GetChildInfo("blendtarget");
            atgiLocatorType.cameraChild = atgiLocatorType.Type.GetChildInfo("camera");
            atgiLocatorType.constraintChild = atgiLocatorType.Type.GetChildInfo("constraint");
            atgiLocatorType.clusterChild = atgiLocatorType.Type.GetChildInfo("cluster");
            atgiLocatorType.dynamicTypeChild = atgiLocatorType.Type.GetChildInfo("dynamicType");
            atgiLocatorType.instanceChild = atgiLocatorType.Type.GetChildInfo("instance");
            atgiLocatorType.jointChild = atgiLocatorType.Type.GetChildInfo("joint");
            atgiLocatorType.lightChild = atgiLocatorType.Type.GetChildInfo("light");
            atgiLocatorType.locatorChild = atgiLocatorType.Type.GetChildInfo("locator");
            atgiLocatorType.lodgroupChild = atgiLocatorType.Type.GetChildInfo("lodgroup");
            atgiLocatorType.meshChild = atgiLocatorType.Type.GetChildInfo("mesh");
            atgiLocatorType.multiBlendTargetChild = atgiLocatorType.Type.GetChildInfo("multiBlendTarget");
            atgiLocatorType.nodeChild = atgiLocatorType.Type.GetChildInfo("node");
            atgiLocatorType.nurbsCurveChild = atgiLocatorType.Type.GetChildInfo("nurbsCurve");
            atgiLocatorType.nurbsChild = atgiLocatorType.Type.GetChildInfo("nurbs");
            atgiLocatorType.orientationConstraintChild = atgiLocatorType.Type.GetChildInfo("orientationConstraint");
            atgiLocatorType.parentConstraintChild = atgiLocatorType.Type.GetChildInfo("parentConstraint");
            atgiLocatorType.primitiveChild = atgiLocatorType.Type.GetChildInfo("primitive");
            atgiLocatorType.referenceChild = atgiLocatorType.Type.GetChildInfo("reference");
            atgiLocatorType.rigidBodyChild = atgiLocatorType.Type.GetChildInfo("rigidBody");
            atgiLocatorType.scaleConstraintChild = atgiLocatorType.Type.GetChildInfo("scaleConstraint");
            atgiLocatorType.springConstraintChild = atgiLocatorType.Type.GetChildInfo("springConstraint");
            atgiLocatorType.translationConstraintChild = atgiLocatorType.Type.GetChildInfo("translationConstraint");
            atgiLocatorType.animclipChild = atgiLocatorType.Type.GetChildInfo("animclip");
            atgiLocatorType.blendChild = atgiLocatorType.Type.GetChildInfo("blend");
            atgiLocatorType.blendshapeControllerChild = atgiLocatorType.Type.GetChildInfo("blendshapeController");
            atgiLocatorType.cgshaderChild = atgiLocatorType.Type.GetChildInfo("cgshader");
            atgiLocatorType.deformerChild = atgiLocatorType.Type.GetChildInfo("deformer");
            atgiLocatorType.expressionChild = atgiLocatorType.Type.GetChildInfo("expression");
            atgiLocatorType.imageChild = atgiLocatorType.Type.GetChildInfo("image");
            atgiLocatorType.materialChild = atgiLocatorType.Type.GetChildInfo("material");
            atgiLocatorType.motionPathChild = atgiLocatorType.Type.GetChildInfo("motionPath");
            atgiLocatorType.objsetChild = atgiLocatorType.Type.GetChildInfo("objset");
            atgiLocatorType.pmdataATGChild = atgiLocatorType.Type.GetChildInfo("pmdataATG");
            atgiLocatorType.poseChild = atgiLocatorType.Type.GetChildInfo("pose");
            atgiLocatorType.sceneChild = atgiLocatorType.Type.GetChildInfo("scene");
            atgiLocatorType.shaderChild = atgiLocatorType.Type.GetChildInfo("shader");
            atgiLocatorType.textureChild = atgiLocatorType.Type.GetChildInfo("texture");
            atgiLocatorType.blendshapeChild = atgiLocatorType.Type.GetChildInfo("blendshape");
            atgiLocatorType.skinChild = atgiLocatorType.Type.GetChildInfo("skin");
            atgiLocatorType.customDataChild = atgiLocatorType.Type.GetChildInfo("customData");

            atgiLocatorType_file.Type = typeCollection.GetNodeType("atgiLocatorType_file");
            atgiLocatorType_file.uriAttribute = atgiLocatorType_file.Type.GetAttributeInfo("uri");

            blendtargetType.Type = typeCollection.GetNodeType("blendtargetType");
            blendtargetType.nameAttribute = blendtargetType.Type.GetAttributeInfo("name");
            blendtargetType.diffChild = blendtargetType.Type.GetChildInfo("diff");

            blendtargetType_diff.Type = typeCollection.GetNodeType("blendtargetType_diff");
            blendtargetType_diff.nameAttribute = blendtargetType_diff.Type.GetAttributeInfo("name");
            blendtargetType_diff.indicesChild = blendtargetType_diff.Type.GetChildInfo("indices");
            blendtargetType_diff.deltasChild = blendtargetType_diff.Type.GetChildInfo("deltas");

            diff_indices.Type = typeCollection.GetNodeType("diff_indices");
            diff_indices.Attribute = diff_indices.Type.GetAttributeInfo("");
            diff_indices.countAttribute = diff_indices.Type.GetAttributeInfo("count");

            diff_deltas.Type = typeCollection.GetNodeType("diff_deltas");
            diff_deltas.Attribute = diff_deltas.Type.GetAttributeInfo("");
            diff_deltas.countAttribute = diff_deltas.Type.GetAttributeInfo("count");

            cameraType.Type = typeCollection.GetNodeType("cameraType");
            cameraType.nameAttribute = cameraType.Type.GetAttributeInfo("name");
            cameraType.nearClipPlaneAttribute = cameraType.Type.GetAttributeInfo("nearClipPlane");
            cameraType.farClipPlaneAttribute = cameraType.Type.GetAttributeInfo("farClipPlane");
            cameraType.focalLengthAttribute = cameraType.Type.GetAttributeInfo("focalLength");
            cameraType.verticalFilmApertureAttribute = cameraType.Type.GetAttributeInfo("verticalFilmAperture");
            cameraType.horizontalFilmApertureAttribute = cameraType.Type.GetAttributeInfo("horizontalFilmAperture");
            cameraType.orthographicAttribute = cameraType.Type.GetAttributeInfo("orthographic");
            cameraType.orthographicWidthAttribute = cameraType.Type.GetAttributeInfo("orthographicWidth");
            cameraType.animChannelChild = cameraType.Type.GetChildInfo("animChannel");
            cameraType.animDiscontinuitiesChild = cameraType.Type.GetChildInfo("animDiscontinuities");
            cameraType.animChild = cameraType.Type.GetChildInfo("anim");
            cameraType.aimConstraintChild = cameraType.Type.GetChildInfo("aimConstraint");
            cameraType.atgilocatorChild = cameraType.Type.GetChildInfo("atgilocator");
            cameraType.blendtargetChild = cameraType.Type.GetChildInfo("blendtarget");
            cameraType.cameraChild = cameraType.Type.GetChildInfo("camera");
            cameraType.constraintChild = cameraType.Type.GetChildInfo("constraint");
            cameraType.clusterChild = cameraType.Type.GetChildInfo("cluster");
            cameraType.dynamicTypeChild = cameraType.Type.GetChildInfo("dynamicType");
            cameraType.instanceChild = cameraType.Type.GetChildInfo("instance");
            cameraType.jointChild = cameraType.Type.GetChildInfo("joint");
            cameraType.lightChild = cameraType.Type.GetChildInfo("light");
            cameraType.locatorChild = cameraType.Type.GetChildInfo("locator");
            cameraType.lodgroupChild = cameraType.Type.GetChildInfo("lodgroup");
            cameraType.meshChild = cameraType.Type.GetChildInfo("mesh");
            cameraType.multiBlendTargetChild = cameraType.Type.GetChildInfo("multiBlendTarget");
            cameraType.nodeChild = cameraType.Type.GetChildInfo("node");
            cameraType.nurbsCurveChild = cameraType.Type.GetChildInfo("nurbsCurve");
            cameraType.nurbsChild = cameraType.Type.GetChildInfo("nurbs");
            cameraType.orientationConstraintChild = cameraType.Type.GetChildInfo("orientationConstraint");
            cameraType.parentConstraintChild = cameraType.Type.GetChildInfo("parentConstraint");
            cameraType.primitiveChild = cameraType.Type.GetChildInfo("primitive");
            cameraType.referenceChild = cameraType.Type.GetChildInfo("reference");
            cameraType.rigidBodyChild = cameraType.Type.GetChildInfo("rigidBody");
            cameraType.scaleConstraintChild = cameraType.Type.GetChildInfo("scaleConstraint");
            cameraType.springConstraintChild = cameraType.Type.GetChildInfo("springConstraint");
            cameraType.translationConstraintChild = cameraType.Type.GetChildInfo("translationConstraint");
            cameraType.animclipChild = cameraType.Type.GetChildInfo("animclip");
            cameraType.blendChild = cameraType.Type.GetChildInfo("blend");
            cameraType.blendshapeControllerChild = cameraType.Type.GetChildInfo("blendshapeController");
            cameraType.cgshaderChild = cameraType.Type.GetChildInfo("cgshader");
            cameraType.deformerChild = cameraType.Type.GetChildInfo("deformer");
            cameraType.expressionChild = cameraType.Type.GetChildInfo("expression");
            cameraType.imageChild = cameraType.Type.GetChildInfo("image");
            cameraType.materialChild = cameraType.Type.GetChildInfo("material");
            cameraType.motionPathChild = cameraType.Type.GetChildInfo("motionPath");
            cameraType.objsetChild = cameraType.Type.GetChildInfo("objset");
            cameraType.pmdataATGChild = cameraType.Type.GetChildInfo("pmdataATG");
            cameraType.poseChild = cameraType.Type.GetChildInfo("pose");
            cameraType.sceneChild = cameraType.Type.GetChildInfo("scene");
            cameraType.shaderChild = cameraType.Type.GetChildInfo("shader");
            cameraType.textureChild = cameraType.Type.GetChildInfo("texture");
            cameraType.blendshapeChild = cameraType.Type.GetChildInfo("blendshape");
            cameraType.skinChild = cameraType.Type.GetChildInfo("skin");
            cameraType.customDataChild = cameraType.Type.GetChildInfo("customData");

            constraintType.Type = typeCollection.GetNodeType("constraintType");
            constraintType.nameAttribute = constraintType.Type.GetAttributeInfo("name");
            constraintType.constrainAttribute = constraintType.Type.GetAttributeInfo("constrain");
            constraintType.targetChild = constraintType.Type.GetChildInfo("target");
            constraintType.animChannelChild = constraintType.Type.GetChildInfo("animChannel");
            constraintType.animDiscontinuitiesChild = constraintType.Type.GetChildInfo("animDiscontinuities");
            constraintType.animChild = constraintType.Type.GetChildInfo("anim");
            constraintType.aimConstraintChild = constraintType.Type.GetChildInfo("aimConstraint");
            constraintType.atgilocatorChild = constraintType.Type.GetChildInfo("atgilocator");
            constraintType.blendtargetChild = constraintType.Type.GetChildInfo("blendtarget");
            constraintType.cameraChild = constraintType.Type.GetChildInfo("camera");
            constraintType.constraintChild = constraintType.Type.GetChildInfo("constraint");
            constraintType.clusterChild = constraintType.Type.GetChildInfo("cluster");
            constraintType.dynamicTypeChild = constraintType.Type.GetChildInfo("dynamicType");
            constraintType.instanceChild = constraintType.Type.GetChildInfo("instance");
            constraintType.jointChild = constraintType.Type.GetChildInfo("joint");
            constraintType.lightChild = constraintType.Type.GetChildInfo("light");
            constraintType.locatorChild = constraintType.Type.GetChildInfo("locator");
            constraintType.lodgroupChild = constraintType.Type.GetChildInfo("lodgroup");
            constraintType.meshChild = constraintType.Type.GetChildInfo("mesh");
            constraintType.multiBlendTargetChild = constraintType.Type.GetChildInfo("multiBlendTarget");
            constraintType.nodeChild = constraintType.Type.GetChildInfo("node");
            constraintType.nurbsCurveChild = constraintType.Type.GetChildInfo("nurbsCurve");
            constraintType.nurbsChild = constraintType.Type.GetChildInfo("nurbs");
            constraintType.orientationConstraintChild = constraintType.Type.GetChildInfo("orientationConstraint");
            constraintType.parentConstraintChild = constraintType.Type.GetChildInfo("parentConstraint");
            constraintType.primitiveChild = constraintType.Type.GetChildInfo("primitive");
            constraintType.referenceChild = constraintType.Type.GetChildInfo("reference");
            constraintType.rigidBodyChild = constraintType.Type.GetChildInfo("rigidBody");
            constraintType.scaleConstraintChild = constraintType.Type.GetChildInfo("scaleConstraint");
            constraintType.springConstraintChild = constraintType.Type.GetChildInfo("springConstraint");
            constraintType.translationConstraintChild = constraintType.Type.GetChildInfo("translationConstraint");
            constraintType.animclipChild = constraintType.Type.GetChildInfo("animclip");
            constraintType.blendChild = constraintType.Type.GetChildInfo("blend");
            constraintType.blendshapeControllerChild = constraintType.Type.GetChildInfo("blendshapeController");
            constraintType.cgshaderChild = constraintType.Type.GetChildInfo("cgshader");
            constraintType.deformerChild = constraintType.Type.GetChildInfo("deformer");
            constraintType.expressionChild = constraintType.Type.GetChildInfo("expression");
            constraintType.imageChild = constraintType.Type.GetChildInfo("image");
            constraintType.materialChild = constraintType.Type.GetChildInfo("material");
            constraintType.motionPathChild = constraintType.Type.GetChildInfo("motionPath");
            constraintType.objsetChild = constraintType.Type.GetChildInfo("objset");
            constraintType.pmdataATGChild = constraintType.Type.GetChildInfo("pmdataATG");
            constraintType.poseChild = constraintType.Type.GetChildInfo("pose");
            constraintType.sceneChild = constraintType.Type.GetChildInfo("scene");
            constraintType.shaderChild = constraintType.Type.GetChildInfo("shader");
            constraintType.textureChild = constraintType.Type.GetChildInfo("texture");
            constraintType.blendshapeChild = constraintType.Type.GetChildInfo("blendshape");
            constraintType.skinChild = constraintType.Type.GetChildInfo("skin");
            constraintType.customDataChild = constraintType.Type.GetChildInfo("customData");

            clusterType.Type = typeCollection.GetNodeType("clusterType");
            clusterType.nameAttribute = clusterType.Type.GetAttributeInfo("name");
            clusterType.animChannelChild = clusterType.Type.GetChildInfo("animChannel");
            clusterType.animDiscontinuitiesChild = clusterType.Type.GetChildInfo("animDiscontinuities");
            clusterType.animChild = clusterType.Type.GetChildInfo("anim");
            clusterType.aimConstraintChild = clusterType.Type.GetChildInfo("aimConstraint");
            clusterType.atgilocatorChild = clusterType.Type.GetChildInfo("atgilocator");
            clusterType.blendtargetChild = clusterType.Type.GetChildInfo("blendtarget");
            clusterType.cameraChild = clusterType.Type.GetChildInfo("camera");
            clusterType.constraintChild = clusterType.Type.GetChildInfo("constraint");
            clusterType.clusterChild = clusterType.Type.GetChildInfo("cluster");
            clusterType.dynamicTypeChild = clusterType.Type.GetChildInfo("dynamicType");
            clusterType.instanceChild = clusterType.Type.GetChildInfo("instance");
            clusterType.jointChild = clusterType.Type.GetChildInfo("joint");
            clusterType.lightChild = clusterType.Type.GetChildInfo("light");
            clusterType.locatorChild = clusterType.Type.GetChildInfo("locator");
            clusterType.lodgroupChild = clusterType.Type.GetChildInfo("lodgroup");
            clusterType.meshChild = clusterType.Type.GetChildInfo("mesh");
            clusterType.multiBlendTargetChild = clusterType.Type.GetChildInfo("multiBlendTarget");
            clusterType.nodeChild = clusterType.Type.GetChildInfo("node");
            clusterType.nurbsCurveChild = clusterType.Type.GetChildInfo("nurbsCurve");
            clusterType.nurbsChild = clusterType.Type.GetChildInfo("nurbs");
            clusterType.orientationConstraintChild = clusterType.Type.GetChildInfo("orientationConstraint");
            clusterType.parentConstraintChild = clusterType.Type.GetChildInfo("parentConstraint");
            clusterType.primitiveChild = clusterType.Type.GetChildInfo("primitive");
            clusterType.referenceChild = clusterType.Type.GetChildInfo("reference");
            clusterType.rigidBodyChild = clusterType.Type.GetChildInfo("rigidBody");
            clusterType.scaleConstraintChild = clusterType.Type.GetChildInfo("scaleConstraint");
            clusterType.springConstraintChild = clusterType.Type.GetChildInfo("springConstraint");
            clusterType.translationConstraintChild = clusterType.Type.GetChildInfo("translationConstraint");
            clusterType.animclipChild = clusterType.Type.GetChildInfo("animclip");
            clusterType.blendChild = clusterType.Type.GetChildInfo("blend");
            clusterType.blendshapeControllerChild = clusterType.Type.GetChildInfo("blendshapeController");
            clusterType.cgshaderChild = clusterType.Type.GetChildInfo("cgshader");
            clusterType.deformerChild = clusterType.Type.GetChildInfo("deformer");
            clusterType.expressionChild = clusterType.Type.GetChildInfo("expression");
            clusterType.imageChild = clusterType.Type.GetChildInfo("image");
            clusterType.materialChild = clusterType.Type.GetChildInfo("material");
            clusterType.motionPathChild = clusterType.Type.GetChildInfo("motionPath");
            clusterType.objsetChild = clusterType.Type.GetChildInfo("objset");
            clusterType.pmdataATGChild = clusterType.Type.GetChildInfo("pmdataATG");
            clusterType.poseChild = clusterType.Type.GetChildInfo("pose");
            clusterType.sceneChild = clusterType.Type.GetChildInfo("scene");
            clusterType.shaderChild = clusterType.Type.GetChildInfo("shader");
            clusterType.textureChild = clusterType.Type.GetChildInfo("texture");
            clusterType.blendshapeChild = clusterType.Type.GetChildInfo("blendshape");
            clusterType.skinChild = clusterType.Type.GetChildInfo("skin");
            clusterType.customDataChild = clusterType.Type.GetChildInfo("customData");

            dynamicTypeType.Type = typeCollection.GetNodeType("dynamicTypeType");

            instanceType.Type = typeCollection.GetNodeType("instanceType");
            instanceType.nameAttribute = instanceType.Type.GetAttributeInfo("name");
            instanceType.targetAttribute = instanceType.Type.GetAttributeInfo("target");
            instanceType.animChannelChild = instanceType.Type.GetChildInfo("animChannel");
            instanceType.animDiscontinuitiesChild = instanceType.Type.GetChildInfo("animDiscontinuities");
            instanceType.animChild = instanceType.Type.GetChildInfo("anim");
            instanceType.aimConstraintChild = instanceType.Type.GetChildInfo("aimConstraint");
            instanceType.atgilocatorChild = instanceType.Type.GetChildInfo("atgilocator");
            instanceType.blendtargetChild = instanceType.Type.GetChildInfo("blendtarget");
            instanceType.cameraChild = instanceType.Type.GetChildInfo("camera");
            instanceType.constraintChild = instanceType.Type.GetChildInfo("constraint");
            instanceType.clusterChild = instanceType.Type.GetChildInfo("cluster");
            instanceType.dynamicTypeChild = instanceType.Type.GetChildInfo("dynamicType");
            instanceType.instanceChild = instanceType.Type.GetChildInfo("instance");
            instanceType.jointChild = instanceType.Type.GetChildInfo("joint");
            instanceType.lightChild = instanceType.Type.GetChildInfo("light");
            instanceType.locatorChild = instanceType.Type.GetChildInfo("locator");
            instanceType.lodgroupChild = instanceType.Type.GetChildInfo("lodgroup");
            instanceType.meshChild = instanceType.Type.GetChildInfo("mesh");
            instanceType.multiBlendTargetChild = instanceType.Type.GetChildInfo("multiBlendTarget");
            instanceType.nodeChild = instanceType.Type.GetChildInfo("node");
            instanceType.nurbsCurveChild = instanceType.Type.GetChildInfo("nurbsCurve");
            instanceType.nurbsChild = instanceType.Type.GetChildInfo("nurbs");
            instanceType.orientationConstraintChild = instanceType.Type.GetChildInfo("orientationConstraint");
            instanceType.parentConstraintChild = instanceType.Type.GetChildInfo("parentConstraint");
            instanceType.primitiveChild = instanceType.Type.GetChildInfo("primitive");
            instanceType.referenceChild = instanceType.Type.GetChildInfo("reference");
            instanceType.rigidBodyChild = instanceType.Type.GetChildInfo("rigidBody");
            instanceType.scaleConstraintChild = instanceType.Type.GetChildInfo("scaleConstraint");
            instanceType.springConstraintChild = instanceType.Type.GetChildInfo("springConstraint");
            instanceType.translationConstraintChild = instanceType.Type.GetChildInfo("translationConstraint");
            instanceType.animclipChild = instanceType.Type.GetChildInfo("animclip");
            instanceType.blendChild = instanceType.Type.GetChildInfo("blend");
            instanceType.blendshapeControllerChild = instanceType.Type.GetChildInfo("blendshapeController");
            instanceType.cgshaderChild = instanceType.Type.GetChildInfo("cgshader");
            instanceType.deformerChild = instanceType.Type.GetChildInfo("deformer");
            instanceType.expressionChild = instanceType.Type.GetChildInfo("expression");
            instanceType.imageChild = instanceType.Type.GetChildInfo("image");
            instanceType.materialChild = instanceType.Type.GetChildInfo("material");
            instanceType.motionPathChild = instanceType.Type.GetChildInfo("motionPath");
            instanceType.objsetChild = instanceType.Type.GetChildInfo("objset");
            instanceType.pmdataATGChild = instanceType.Type.GetChildInfo("pmdataATG");
            instanceType.poseChild = instanceType.Type.GetChildInfo("pose");
            instanceType.sceneChild = instanceType.Type.GetChildInfo("scene");
            instanceType.shaderChild = instanceType.Type.GetChildInfo("shader");
            instanceType.textureChild = instanceType.Type.GetChildInfo("texture");
            instanceType.blendshapeChild = instanceType.Type.GetChildInfo("blendshape");
            instanceType.skinChild = instanceType.Type.GetChildInfo("skin");
            instanceType.customDataChild = instanceType.Type.GetChildInfo("customData");

            jointType.Type = typeCollection.GetNodeType("jointType");
            jointType.nameAttribute = jointType.Type.GetAttributeInfo("name");
            jointType.translateAttribute = jointType.Type.GetAttributeInfo("translate");
            jointType.scaleAttribute = jointType.Type.GetAttributeInfo("scale");
            jointType.shearAttribute = jointType.Type.GetAttributeInfo("shear");
            jointType.scalePivotAttribute = jointType.Type.GetAttributeInfo("scalePivot");
            jointType.scalePivotTranslationAttribute = jointType.Type.GetAttributeInfo("scalePivotTranslation");
            jointType.rotatePivotAttribute = jointType.Type.GetAttributeInfo("rotatePivot");
            jointType.rotatePivotTranslationAttribute = jointType.Type.GetAttributeInfo("rotatePivotTranslation");
            jointType.transformAttribute = jointType.Type.GetAttributeInfo("transform");
            jointType.boundingBoxAttribute = jointType.Type.GetAttributeInfo("boundingBox");
            jointType.visibilityAttribute = jointType.Type.GetAttributeInfo("visibility");
            jointType.parentEffectAttribute = jointType.Type.GetAttributeInfo("parentEffect");
            jointType.scaleCompensateAttribute = jointType.Type.GetAttributeInfo("scaleCompensate");
            jointType.rotEulChild = jointType.Type.GetChildInfo("rotEul");
            jointType.rotAxisEulChild = jointType.Type.GetChildInfo("rotAxisEul");
            jointType.animChannelChild = jointType.Type.GetChildInfo("animChannel");
            jointType.animDiscontinuitiesChild = jointType.Type.GetChildInfo("animDiscontinuities");
            jointType.animChild = jointType.Type.GetChildInfo("anim");
            jointType.aimConstraintChild = jointType.Type.GetChildInfo("aimConstraint");
            jointType.atgilocatorChild = jointType.Type.GetChildInfo("atgilocator");
            jointType.blendtargetChild = jointType.Type.GetChildInfo("blendtarget");
            jointType.cameraChild = jointType.Type.GetChildInfo("camera");
            jointType.constraintChild = jointType.Type.GetChildInfo("constraint");
            jointType.clusterChild = jointType.Type.GetChildInfo("cluster");
            jointType.dynamicTypeChild = jointType.Type.GetChildInfo("dynamicType");
            jointType.instanceChild = jointType.Type.GetChildInfo("instance");
            jointType.jointChild = jointType.Type.GetChildInfo("joint");
            jointType.lightChild = jointType.Type.GetChildInfo("light");
            jointType.locatorChild = jointType.Type.GetChildInfo("locator");
            jointType.lodgroupChild = jointType.Type.GetChildInfo("lodgroup");
            jointType.meshChild = jointType.Type.GetChildInfo("mesh");
            jointType.multiBlendTargetChild = jointType.Type.GetChildInfo("multiBlendTarget");
            jointType.nodeChild = jointType.Type.GetChildInfo("node");
            jointType.nurbsCurveChild = jointType.Type.GetChildInfo("nurbsCurve");
            jointType.nurbsChild = jointType.Type.GetChildInfo("nurbs");
            jointType.orientationConstraintChild = jointType.Type.GetChildInfo("orientationConstraint");
            jointType.parentConstraintChild = jointType.Type.GetChildInfo("parentConstraint");
            jointType.primitiveChild = jointType.Type.GetChildInfo("primitive");
            jointType.referenceChild = jointType.Type.GetChildInfo("reference");
            jointType.rigidBodyChild = jointType.Type.GetChildInfo("rigidBody");
            jointType.scaleConstraintChild = jointType.Type.GetChildInfo("scaleConstraint");
            jointType.springConstraintChild = jointType.Type.GetChildInfo("springConstraint");
            jointType.translationConstraintChild = jointType.Type.GetChildInfo("translationConstraint");
            jointType.animclipChild = jointType.Type.GetChildInfo("animclip");
            jointType.blendChild = jointType.Type.GetChildInfo("blend");
            jointType.blendshapeControllerChild = jointType.Type.GetChildInfo("blendshapeController");
            jointType.cgshaderChild = jointType.Type.GetChildInfo("cgshader");
            jointType.deformerChild = jointType.Type.GetChildInfo("deformer");
            jointType.expressionChild = jointType.Type.GetChildInfo("expression");
            jointType.imageChild = jointType.Type.GetChildInfo("image");
            jointType.materialChild = jointType.Type.GetChildInfo("material");
            jointType.motionPathChild = jointType.Type.GetChildInfo("motionPath");
            jointType.objsetChild = jointType.Type.GetChildInfo("objset");
            jointType.pmdataATGChild = jointType.Type.GetChildInfo("pmdataATG");
            jointType.poseChild = jointType.Type.GetChildInfo("pose");
            jointType.sceneChild = jointType.Type.GetChildInfo("scene");
            jointType.shaderChild = jointType.Type.GetChildInfo("shader");
            jointType.textureChild = jointType.Type.GetChildInfo("texture");
            jointType.blendshapeChild = jointType.Type.GetChildInfo("blendshape");
            jointType.skinChild = jointType.Type.GetChildInfo("skin");
            jointType.customDataChild = jointType.Type.GetChildInfo("customData");
            jointType.freedomsChild = jointType.Type.GetChildInfo("freedoms");
            jointType.minrotationChild = jointType.Type.GetChildInfo("minrotation");
            jointType.maxrotationChild = jointType.Type.GetChildInfo("maxrotation");
            jointType.jointOrientEulChild = jointType.Type.GetChildInfo("jointOrientEul");

            nodeType.Type = typeCollection.GetNodeType("nodeType");
            nodeType.nameAttribute = nodeType.Type.GetAttributeInfo("name");
            nodeType.translateAttribute = nodeType.Type.GetAttributeInfo("translate");
            nodeType.scaleAttribute = nodeType.Type.GetAttributeInfo("scale");
            nodeType.shearAttribute = nodeType.Type.GetAttributeInfo("shear");
            nodeType.scalePivotAttribute = nodeType.Type.GetAttributeInfo("scalePivot");
            nodeType.scalePivotTranslationAttribute = nodeType.Type.GetAttributeInfo("scalePivotTranslation");
            nodeType.rotatePivotAttribute = nodeType.Type.GetAttributeInfo("rotatePivot");
            nodeType.rotatePivotTranslationAttribute = nodeType.Type.GetAttributeInfo("rotatePivotTranslation");
            nodeType.transformAttribute = nodeType.Type.GetAttributeInfo("transform");
            nodeType.boundingBoxAttribute = nodeType.Type.GetAttributeInfo("boundingBox");
            nodeType.visibilityAttribute = nodeType.Type.GetAttributeInfo("visibility");
            nodeType.parentEffectAttribute = nodeType.Type.GetAttributeInfo("parentEffect");
            nodeType.rotEulChild = nodeType.Type.GetChildInfo("rotEul");
            nodeType.rotAxisEulChild = nodeType.Type.GetChildInfo("rotAxisEul");
            nodeType.animChannelChild = nodeType.Type.GetChildInfo("animChannel");
            nodeType.animDiscontinuitiesChild = nodeType.Type.GetChildInfo("animDiscontinuities");
            nodeType.animChild = nodeType.Type.GetChildInfo("anim");
            nodeType.aimConstraintChild = nodeType.Type.GetChildInfo("aimConstraint");
            nodeType.atgilocatorChild = nodeType.Type.GetChildInfo("atgilocator");
            nodeType.blendtargetChild = nodeType.Type.GetChildInfo("blendtarget");
            nodeType.cameraChild = nodeType.Type.GetChildInfo("camera");
            nodeType.constraintChild = nodeType.Type.GetChildInfo("constraint");
            nodeType.clusterChild = nodeType.Type.GetChildInfo("cluster");
            nodeType.dynamicTypeChild = nodeType.Type.GetChildInfo("dynamicType");
            nodeType.instanceChild = nodeType.Type.GetChildInfo("instance");
            nodeType.jointChild = nodeType.Type.GetChildInfo("joint");
            nodeType.lightChild = nodeType.Type.GetChildInfo("light");
            nodeType.locatorChild = nodeType.Type.GetChildInfo("locator");
            nodeType.lodgroupChild = nodeType.Type.GetChildInfo("lodgroup");
            nodeType.meshChild = nodeType.Type.GetChildInfo("mesh");
            nodeType.multiBlendTargetChild = nodeType.Type.GetChildInfo("multiBlendTarget");
            nodeType.nodeChild = nodeType.Type.GetChildInfo("node");
            nodeType.nurbsCurveChild = nodeType.Type.GetChildInfo("nurbsCurve");
            nodeType.nurbsChild = nodeType.Type.GetChildInfo("nurbs");
            nodeType.orientationConstraintChild = nodeType.Type.GetChildInfo("orientationConstraint");
            nodeType.parentConstraintChild = nodeType.Type.GetChildInfo("parentConstraint");
            nodeType.primitiveChild = nodeType.Type.GetChildInfo("primitive");
            nodeType.referenceChild = nodeType.Type.GetChildInfo("reference");
            nodeType.rigidBodyChild = nodeType.Type.GetChildInfo("rigidBody");
            nodeType.scaleConstraintChild = nodeType.Type.GetChildInfo("scaleConstraint");
            nodeType.springConstraintChild = nodeType.Type.GetChildInfo("springConstraint");
            nodeType.translationConstraintChild = nodeType.Type.GetChildInfo("translationConstraint");
            nodeType.animclipChild = nodeType.Type.GetChildInfo("animclip");
            nodeType.blendChild = nodeType.Type.GetChildInfo("blend");
            nodeType.blendshapeControllerChild = nodeType.Type.GetChildInfo("blendshapeController");
            nodeType.cgshaderChild = nodeType.Type.GetChildInfo("cgshader");
            nodeType.deformerChild = nodeType.Type.GetChildInfo("deformer");
            nodeType.expressionChild = nodeType.Type.GetChildInfo("expression");
            nodeType.imageChild = nodeType.Type.GetChildInfo("image");
            nodeType.materialChild = nodeType.Type.GetChildInfo("material");
            nodeType.motionPathChild = nodeType.Type.GetChildInfo("motionPath");
            nodeType.objsetChild = nodeType.Type.GetChildInfo("objset");
            nodeType.pmdataATGChild = nodeType.Type.GetChildInfo("pmdataATG");
            nodeType.poseChild = nodeType.Type.GetChildInfo("pose");
            nodeType.sceneChild = nodeType.Type.GetChildInfo("scene");
            nodeType.shaderChild = nodeType.Type.GetChildInfo("shader");
            nodeType.textureChild = nodeType.Type.GetChildInfo("texture");
            nodeType.blendshapeChild = nodeType.Type.GetChildInfo("blendshape");
            nodeType.skinChild = nodeType.Type.GetChildInfo("skin");
            nodeType.customDataChild = nodeType.Type.GetChildInfo("customData");

            lightType.Type = typeCollection.GetNodeType("lightType");
            lightType.nameAttribute = lightType.Type.GetAttributeInfo("name");
            lightType.intensityAttribute = lightType.Type.GetAttributeInfo("intensity");
            lightType.colourAttribute = lightType.Type.GetAttributeInfo("colour");
            lightType.colourTextureAttribute = lightType.Type.GetAttributeInfo("colourTexture");
            lightType.coneAngleAttribute = lightType.Type.GetAttributeInfo("coneAngle");
            lightType.penumbraAngleAttribute = lightType.Type.GetAttributeInfo("penumbraAngle");
            lightType.dropOffAttribute = lightType.Type.GetAttributeInfo("dropOff");
            lightType.decayRateAttribute = lightType.Type.GetAttributeInfo("decayRate");
            lightType.scaleAttribute = lightType.Type.GetAttributeInfo("scale");
            lightType.typeAttribute = lightType.Type.GetAttributeInfo("type");
            lightType.castShadowsAttribute = lightType.Type.GetAttributeInfo("castShadows");
            lightType.hasAmbientAttribute = lightType.Type.GetAttributeInfo("hasAmbient");
            lightType.hasDiffuseAttribute = lightType.Type.GetAttributeInfo("hasDiffuse");
            lightType.hasSpecularAttribute = lightType.Type.GetAttributeInfo("hasSpecular");
            lightType.colourRampChild = lightType.Type.GetChildInfo("colourRamp");
            lightType.animChannelChild = lightType.Type.GetChildInfo("animChannel");
            lightType.animDiscontinuitiesChild = lightType.Type.GetChildInfo("animDiscontinuities");
            lightType.animChild = lightType.Type.GetChildInfo("anim");
            lightType.aimConstraintChild = lightType.Type.GetChildInfo("aimConstraint");
            lightType.atgilocatorChild = lightType.Type.GetChildInfo("atgilocator");
            lightType.blendtargetChild = lightType.Type.GetChildInfo("blendtarget");
            lightType.cameraChild = lightType.Type.GetChildInfo("camera");
            lightType.constraintChild = lightType.Type.GetChildInfo("constraint");
            lightType.clusterChild = lightType.Type.GetChildInfo("cluster");
            lightType.dynamicTypeChild = lightType.Type.GetChildInfo("dynamicType");
            lightType.instanceChild = lightType.Type.GetChildInfo("instance");
            lightType.jointChild = lightType.Type.GetChildInfo("joint");
            lightType.lightChild = lightType.Type.GetChildInfo("light");
            lightType.locatorChild = lightType.Type.GetChildInfo("locator");
            lightType.lodgroupChild = lightType.Type.GetChildInfo("lodgroup");
            lightType.meshChild = lightType.Type.GetChildInfo("mesh");
            lightType.multiBlendTargetChild = lightType.Type.GetChildInfo("multiBlendTarget");
            lightType.nodeChild = lightType.Type.GetChildInfo("node");
            lightType.nurbsCurveChild = lightType.Type.GetChildInfo("nurbsCurve");
            lightType.nurbsChild = lightType.Type.GetChildInfo("nurbs");
            lightType.orientationConstraintChild = lightType.Type.GetChildInfo("orientationConstraint");
            lightType.parentConstraintChild = lightType.Type.GetChildInfo("parentConstraint");
            lightType.primitiveChild = lightType.Type.GetChildInfo("primitive");
            lightType.referenceChild = lightType.Type.GetChildInfo("reference");
            lightType.rigidBodyChild = lightType.Type.GetChildInfo("rigidBody");
            lightType.scaleConstraintChild = lightType.Type.GetChildInfo("scaleConstraint");
            lightType.springConstraintChild = lightType.Type.GetChildInfo("springConstraint");
            lightType.translationConstraintChild = lightType.Type.GetChildInfo("translationConstraint");
            lightType.animclipChild = lightType.Type.GetChildInfo("animclip");
            lightType.blendChild = lightType.Type.GetChildInfo("blend");
            lightType.blendshapeControllerChild = lightType.Type.GetChildInfo("blendshapeController");
            lightType.cgshaderChild = lightType.Type.GetChildInfo("cgshader");
            lightType.deformerChild = lightType.Type.GetChildInfo("deformer");
            lightType.expressionChild = lightType.Type.GetChildInfo("expression");
            lightType.imageChild = lightType.Type.GetChildInfo("image");
            lightType.materialChild = lightType.Type.GetChildInfo("material");
            lightType.motionPathChild = lightType.Type.GetChildInfo("motionPath");
            lightType.objsetChild = lightType.Type.GetChildInfo("objset");
            lightType.pmdataATGChild = lightType.Type.GetChildInfo("pmdataATG");
            lightType.poseChild = lightType.Type.GetChildInfo("pose");
            lightType.sceneChild = lightType.Type.GetChildInfo("scene");
            lightType.shaderChild = lightType.Type.GetChildInfo("shader");
            lightType.textureChild = lightType.Type.GetChildInfo("texture");
            lightType.blendshapeChild = lightType.Type.GetChildInfo("blendshape");
            lightType.skinChild = lightType.Type.GetChildInfo("skin");
            lightType.customDataChild = lightType.Type.GetChildInfo("customData");

            lightType_colourRamp.Type = typeCollection.GetNodeType("lightType_colourRamp");
            lightType_colourRamp.positionsAttribute = lightType_colourRamp.Type.GetAttributeInfo("positions");
            lightType_colourRamp.coloursAttribute = lightType_colourRamp.Type.GetAttributeInfo("colours");
            lightType_colourRamp.interpolationTypesAttribute = lightType_colourRamp.Type.GetAttributeInfo("interpolationTypes");

            locatorType.Type = typeCollection.GetNodeType("locatorType");
            locatorType.nameAttribute = locatorType.Type.GetAttributeInfo("name");
            locatorType.localPositionAttribute = locatorType.Type.GetAttributeInfo("localPosition");
            locatorType.animChannelChild = locatorType.Type.GetChildInfo("animChannel");
            locatorType.animDiscontinuitiesChild = locatorType.Type.GetChildInfo("animDiscontinuities");
            locatorType.animChild = locatorType.Type.GetChildInfo("anim");
            locatorType.aimConstraintChild = locatorType.Type.GetChildInfo("aimConstraint");
            locatorType.atgilocatorChild = locatorType.Type.GetChildInfo("atgilocator");
            locatorType.blendtargetChild = locatorType.Type.GetChildInfo("blendtarget");
            locatorType.cameraChild = locatorType.Type.GetChildInfo("camera");
            locatorType.constraintChild = locatorType.Type.GetChildInfo("constraint");
            locatorType.clusterChild = locatorType.Type.GetChildInfo("cluster");
            locatorType.dynamicTypeChild = locatorType.Type.GetChildInfo("dynamicType");
            locatorType.instanceChild = locatorType.Type.GetChildInfo("instance");
            locatorType.jointChild = locatorType.Type.GetChildInfo("joint");
            locatorType.lightChild = locatorType.Type.GetChildInfo("light");
            locatorType.locatorChild = locatorType.Type.GetChildInfo("locator");
            locatorType.lodgroupChild = locatorType.Type.GetChildInfo("lodgroup");
            locatorType.meshChild = locatorType.Type.GetChildInfo("mesh");
            locatorType.multiBlendTargetChild = locatorType.Type.GetChildInfo("multiBlendTarget");
            locatorType.nodeChild = locatorType.Type.GetChildInfo("node");
            locatorType.nurbsCurveChild = locatorType.Type.GetChildInfo("nurbsCurve");
            locatorType.nurbsChild = locatorType.Type.GetChildInfo("nurbs");
            locatorType.orientationConstraintChild = locatorType.Type.GetChildInfo("orientationConstraint");
            locatorType.parentConstraintChild = locatorType.Type.GetChildInfo("parentConstraint");
            locatorType.primitiveChild = locatorType.Type.GetChildInfo("primitive");
            locatorType.referenceChild = locatorType.Type.GetChildInfo("reference");
            locatorType.rigidBodyChild = locatorType.Type.GetChildInfo("rigidBody");
            locatorType.scaleConstraintChild = locatorType.Type.GetChildInfo("scaleConstraint");
            locatorType.springConstraintChild = locatorType.Type.GetChildInfo("springConstraint");
            locatorType.translationConstraintChild = locatorType.Type.GetChildInfo("translationConstraint");
            locatorType.animclipChild = locatorType.Type.GetChildInfo("animclip");
            locatorType.blendChild = locatorType.Type.GetChildInfo("blend");
            locatorType.blendshapeControllerChild = locatorType.Type.GetChildInfo("blendshapeController");
            locatorType.cgshaderChild = locatorType.Type.GetChildInfo("cgshader");
            locatorType.deformerChild = locatorType.Type.GetChildInfo("deformer");
            locatorType.expressionChild = locatorType.Type.GetChildInfo("expression");
            locatorType.imageChild = locatorType.Type.GetChildInfo("image");
            locatorType.materialChild = locatorType.Type.GetChildInfo("material");
            locatorType.motionPathChild = locatorType.Type.GetChildInfo("motionPath");
            locatorType.objsetChild = locatorType.Type.GetChildInfo("objset");
            locatorType.pmdataATGChild = locatorType.Type.GetChildInfo("pmdataATG");
            locatorType.poseChild = locatorType.Type.GetChildInfo("pose");
            locatorType.sceneChild = locatorType.Type.GetChildInfo("scene");
            locatorType.shaderChild = locatorType.Type.GetChildInfo("shader");
            locatorType.textureChild = locatorType.Type.GetChildInfo("texture");
            locatorType.blendshapeChild = locatorType.Type.GetChildInfo("blendshape");
            locatorType.skinChild = locatorType.Type.GetChildInfo("skin");
            locatorType.customDataChild = locatorType.Type.GetChildInfo("customData");

            lodgroupType.Type = typeCollection.GetNodeType("lodgroupType");
            lodgroupType.nameAttribute = lodgroupType.Type.GetAttributeInfo("name");
            lodgroupType.translateAttribute = lodgroupType.Type.GetAttributeInfo("translate");
            lodgroupType.scaleAttribute = lodgroupType.Type.GetAttributeInfo("scale");
            lodgroupType.shearAttribute = lodgroupType.Type.GetAttributeInfo("shear");
            lodgroupType.scalePivotAttribute = lodgroupType.Type.GetAttributeInfo("scalePivot");
            lodgroupType.scalePivotTranslationAttribute = lodgroupType.Type.GetAttributeInfo("scalePivotTranslation");
            lodgroupType.rotatePivotAttribute = lodgroupType.Type.GetAttributeInfo("rotatePivot");
            lodgroupType.rotatePivotTranslationAttribute = lodgroupType.Type.GetAttributeInfo("rotatePivotTranslation");
            lodgroupType.thresholdsChild = lodgroupType.Type.GetChildInfo("thresholds");
            lodgroupType.rotEulChild = lodgroupType.Type.GetChildInfo("rotEul");
            lodgroupType.rotAxisEulChild = lodgroupType.Type.GetChildInfo("rotAxisEul");
            lodgroupType.animChannelChild = lodgroupType.Type.GetChildInfo("animChannel");
            lodgroupType.animDiscontinuitiesChild = lodgroupType.Type.GetChildInfo("animDiscontinuities");
            lodgroupType.animChild = lodgroupType.Type.GetChildInfo("anim");
            lodgroupType.aimConstraintChild = lodgroupType.Type.GetChildInfo("aimConstraint");
            lodgroupType.atgilocatorChild = lodgroupType.Type.GetChildInfo("atgilocator");
            lodgroupType.blendtargetChild = lodgroupType.Type.GetChildInfo("blendtarget");
            lodgroupType.cameraChild = lodgroupType.Type.GetChildInfo("camera");
            lodgroupType.constraintChild = lodgroupType.Type.GetChildInfo("constraint");
            lodgroupType.clusterChild = lodgroupType.Type.GetChildInfo("cluster");
            lodgroupType.dynamicTypeChild = lodgroupType.Type.GetChildInfo("dynamicType");
            lodgroupType.instanceChild = lodgroupType.Type.GetChildInfo("instance");
            lodgroupType.jointChild = lodgroupType.Type.GetChildInfo("joint");
            lodgroupType.lightChild = lodgroupType.Type.GetChildInfo("light");
            lodgroupType.locatorChild = lodgroupType.Type.GetChildInfo("locator");
            lodgroupType.lodgroupChild = lodgroupType.Type.GetChildInfo("lodgroup");
            lodgroupType.meshChild = lodgroupType.Type.GetChildInfo("mesh");
            lodgroupType.multiBlendTargetChild = lodgroupType.Type.GetChildInfo("multiBlendTarget");
            lodgroupType.nodeChild = lodgroupType.Type.GetChildInfo("node");
            lodgroupType.nurbsCurveChild = lodgroupType.Type.GetChildInfo("nurbsCurve");
            lodgroupType.nurbsChild = lodgroupType.Type.GetChildInfo("nurbs");
            lodgroupType.orientationConstraintChild = lodgroupType.Type.GetChildInfo("orientationConstraint");
            lodgroupType.parentConstraintChild = lodgroupType.Type.GetChildInfo("parentConstraint");
            lodgroupType.primitiveChild = lodgroupType.Type.GetChildInfo("primitive");
            lodgroupType.referenceChild = lodgroupType.Type.GetChildInfo("reference");
            lodgroupType.rigidBodyChild = lodgroupType.Type.GetChildInfo("rigidBody");
            lodgroupType.scaleConstraintChild = lodgroupType.Type.GetChildInfo("scaleConstraint");
            lodgroupType.springConstraintChild = lodgroupType.Type.GetChildInfo("springConstraint");
            lodgroupType.translationConstraintChild = lodgroupType.Type.GetChildInfo("translationConstraint");
            lodgroupType.animclipChild = lodgroupType.Type.GetChildInfo("animclip");
            lodgroupType.blendChild = lodgroupType.Type.GetChildInfo("blend");
            lodgroupType.blendshapeControllerChild = lodgroupType.Type.GetChildInfo("blendshapeController");
            lodgroupType.cgshaderChild = lodgroupType.Type.GetChildInfo("cgshader");
            lodgroupType.deformerChild = lodgroupType.Type.GetChildInfo("deformer");
            lodgroupType.expressionChild = lodgroupType.Type.GetChildInfo("expression");
            lodgroupType.imageChild = lodgroupType.Type.GetChildInfo("image");
            lodgroupType.materialChild = lodgroupType.Type.GetChildInfo("material");
            lodgroupType.motionPathChild = lodgroupType.Type.GetChildInfo("motionPath");
            lodgroupType.objsetChild = lodgroupType.Type.GetChildInfo("objset");
            lodgroupType.pmdataATGChild = lodgroupType.Type.GetChildInfo("pmdataATG");
            lodgroupType.poseChild = lodgroupType.Type.GetChildInfo("pose");
            lodgroupType.sceneChild = lodgroupType.Type.GetChildInfo("scene");
            lodgroupType.shaderChild = lodgroupType.Type.GetChildInfo("shader");
            lodgroupType.textureChild = lodgroupType.Type.GetChildInfo("texture");
            lodgroupType.blendshapeChild = lodgroupType.Type.GetChildInfo("blendshape");
            lodgroupType.skinChild = lodgroupType.Type.GetChildInfo("skin");
            lodgroupType.customDataChild = lodgroupType.Type.GetChildInfo("customData");

            lodgroupType_thresholds.Type = typeCollection.GetNodeType("lodgroupType_thresholds");
            lodgroupType_thresholds.Attribute = lodgroupType_thresholds.Type.GetAttributeInfo("");
            lodgroupType_thresholds.countAttribute = lodgroupType_thresholds.Type.GetAttributeInfo("count");

            meshType.Type = typeCollection.GetNodeType("meshType");
            meshType.nameAttribute = meshType.Type.GetAttributeInfo("name");
            meshType.boundingBoxAttribute = meshType.Type.GetAttributeInfo("boundingBox");
            meshType.vertexArrayChild = meshType.Type.GetChildInfo("vertexArray");
            meshType.animChannelChild = meshType.Type.GetChildInfo("animChannel");
            meshType.animDiscontinuitiesChild = meshType.Type.GetChildInfo("animDiscontinuities");
            meshType.animChild = meshType.Type.GetChildInfo("anim");
            meshType.aimConstraintChild = meshType.Type.GetChildInfo("aimConstraint");
            meshType.atgilocatorChild = meshType.Type.GetChildInfo("atgilocator");
            meshType.blendtargetChild = meshType.Type.GetChildInfo("blendtarget");
            meshType.cameraChild = meshType.Type.GetChildInfo("camera");
            meshType.constraintChild = meshType.Type.GetChildInfo("constraint");
            meshType.clusterChild = meshType.Type.GetChildInfo("cluster");
            meshType.dynamicTypeChild = meshType.Type.GetChildInfo("dynamicType");
            meshType.instanceChild = meshType.Type.GetChildInfo("instance");
            meshType.jointChild = meshType.Type.GetChildInfo("joint");
            meshType.lightChild = meshType.Type.GetChildInfo("light");
            meshType.locatorChild = meshType.Type.GetChildInfo("locator");
            meshType.lodgroupChild = meshType.Type.GetChildInfo("lodgroup");
            meshType.meshChild = meshType.Type.GetChildInfo("mesh");
            meshType.multiBlendTargetChild = meshType.Type.GetChildInfo("multiBlendTarget");
            meshType.nodeChild = meshType.Type.GetChildInfo("node");
            meshType.nurbsCurveChild = meshType.Type.GetChildInfo("nurbsCurve");
            meshType.nurbsChild = meshType.Type.GetChildInfo("nurbs");
            meshType.orientationConstraintChild = meshType.Type.GetChildInfo("orientationConstraint");
            meshType.parentConstraintChild = meshType.Type.GetChildInfo("parentConstraint");
            meshType.primitiveChild = meshType.Type.GetChildInfo("primitive");
            meshType.referenceChild = meshType.Type.GetChildInfo("reference");
            meshType.rigidBodyChild = meshType.Type.GetChildInfo("rigidBody");
            meshType.scaleConstraintChild = meshType.Type.GetChildInfo("scaleConstraint");
            meshType.springConstraintChild = meshType.Type.GetChildInfo("springConstraint");
            meshType.translationConstraintChild = meshType.Type.GetChildInfo("translationConstraint");
            meshType.animclipChild = meshType.Type.GetChildInfo("animclip");
            meshType.blendChild = meshType.Type.GetChildInfo("blend");
            meshType.blendshapeControllerChild = meshType.Type.GetChildInfo("blendshapeController");
            meshType.cgshaderChild = meshType.Type.GetChildInfo("cgshader");
            meshType.deformerChild = meshType.Type.GetChildInfo("deformer");
            meshType.expressionChild = meshType.Type.GetChildInfo("expression");
            meshType.imageChild = meshType.Type.GetChildInfo("image");
            meshType.materialChild = meshType.Type.GetChildInfo("material");
            meshType.motionPathChild = meshType.Type.GetChildInfo("motionPath");
            meshType.objsetChild = meshType.Type.GetChildInfo("objset");
            meshType.pmdataATGChild = meshType.Type.GetChildInfo("pmdataATG");
            meshType.poseChild = meshType.Type.GetChildInfo("pose");
            meshType.sceneChild = meshType.Type.GetChildInfo("scene");
            meshType.shaderChild = meshType.Type.GetChildInfo("shader");
            meshType.textureChild = meshType.Type.GetChildInfo("texture");
            meshType.blendshapeChild = meshType.Type.GetChildInfo("blendshape");
            meshType.skinChild = meshType.Type.GetChildInfo("skin");
            meshType.customDataChild = meshType.Type.GetChildInfo("customData");

            meshType_vertexArray.Type = typeCollection.GetNodeType("meshType_vertexArray");
            meshType_vertexArray.primitivesChild = meshType_vertexArray.Type.GetChildInfo("primitives");
            meshType_vertexArray.arrayChild = meshType_vertexArray.Type.GetChildInfo("array");
            meshType_vertexArray.blindDataChild = meshType_vertexArray.Type.GetChildInfo("blindData");

            vertexArray_primitives.Type = typeCollection.GetNodeType("vertexArray_primitives");
            vertexArray_primitives.sizesAttribute = vertexArray_primitives.Type.GetAttributeInfo("sizes");
            vertexArray_primitives.indicesAttribute = vertexArray_primitives.Type.GetAttributeInfo("indices");
            vertexArray_primitives.nameAttribute = vertexArray_primitives.Type.GetAttributeInfo("name");
            vertexArray_primitives.shaderAttribute = vertexArray_primitives.Type.GetAttributeInfo("shader");
            vertexArray_primitives.typeAttribute = vertexArray_primitives.Type.GetAttributeInfo("type");
            vertexArray_primitives.countAttribute = vertexArray_primitives.Type.GetAttributeInfo("count");
            vertexArray_primitives.bindingChild = vertexArray_primitives.Type.GetChildInfo("binding");

            primitives_binding.Type = typeCollection.GetNodeType("primitives_binding");
            primitives_binding.sourceAttribute = primitives_binding.Type.GetAttributeInfo("source");

            vertexArray_array.Type = typeCollection.GetNodeType("vertexArray_array");
            vertexArray_array.Attribute = vertexArray_array.Type.GetAttributeInfo("");
            vertexArray_array.nameAttribute = vertexArray_array.Type.GetAttributeInfo("name");
            vertexArray_array.countAttribute = vertexArray_array.Type.GetAttributeInfo("count");
            vertexArray_array.strideAttribute = vertexArray_array.Type.GetAttributeInfo("stride");

            blindDataType.Type = typeCollection.GetNodeType("blindDataType");
            blindDataType.nameAttribute = blindDataType.Type.GetAttributeInfo("name");
            blindDataType.componentsAttribute = blindDataType.Type.GetAttributeInfo("components");
            blindDataType.componentAttribute = blindDataType.Type.GetAttributeInfo("component");
            blindDataType.numComponentsAttribute = blindDataType.Type.GetAttributeInfo("numComponents");
            blindDataType.attributeChild = blindDataType.Type.GetChildInfo("attribute");

            blindDataType_attribute.Type = typeCollection.GetNodeType("blindDataType_attribute");
            blindDataType_attribute.Attribute = blindDataType_attribute.Type.GetAttributeInfo("");
            blindDataType_attribute.nameAttribute = blindDataType_attribute.Type.GetAttributeInfo("name");
            blindDataType_attribute.typeAttribute = blindDataType_attribute.Type.GetAttributeInfo("type");

            multiBlendTargetType.Type = typeCollection.GetNodeType("multiBlendTargetType");
            multiBlendTargetType.nameAttribute = multiBlendTargetType.Type.GetAttributeInfo("name");
            multiBlendTargetType.blendStageChild = multiBlendTargetType.Type.GetChildInfo("blendStage");
            multiBlendTargetType.animChannelChild = multiBlendTargetType.Type.GetChildInfo("animChannel");
            multiBlendTargetType.animDiscontinuitiesChild = multiBlendTargetType.Type.GetChildInfo("animDiscontinuities");
            multiBlendTargetType.animChild = multiBlendTargetType.Type.GetChildInfo("anim");
            multiBlendTargetType.aimConstraintChild = multiBlendTargetType.Type.GetChildInfo("aimConstraint");
            multiBlendTargetType.atgilocatorChild = multiBlendTargetType.Type.GetChildInfo("atgilocator");
            multiBlendTargetType.blendtargetChild = multiBlendTargetType.Type.GetChildInfo("blendtarget");
            multiBlendTargetType.cameraChild = multiBlendTargetType.Type.GetChildInfo("camera");
            multiBlendTargetType.constraintChild = multiBlendTargetType.Type.GetChildInfo("constraint");
            multiBlendTargetType.clusterChild = multiBlendTargetType.Type.GetChildInfo("cluster");
            multiBlendTargetType.dynamicTypeChild = multiBlendTargetType.Type.GetChildInfo("dynamicType");
            multiBlendTargetType.instanceChild = multiBlendTargetType.Type.GetChildInfo("instance");
            multiBlendTargetType.jointChild = multiBlendTargetType.Type.GetChildInfo("joint");
            multiBlendTargetType.lightChild = multiBlendTargetType.Type.GetChildInfo("light");
            multiBlendTargetType.locatorChild = multiBlendTargetType.Type.GetChildInfo("locator");
            multiBlendTargetType.lodgroupChild = multiBlendTargetType.Type.GetChildInfo("lodgroup");
            multiBlendTargetType.meshChild = multiBlendTargetType.Type.GetChildInfo("mesh");
            multiBlendTargetType.multiBlendTargetChild = multiBlendTargetType.Type.GetChildInfo("multiBlendTarget");
            multiBlendTargetType.nodeChild = multiBlendTargetType.Type.GetChildInfo("node");
            multiBlendTargetType.nurbsCurveChild = multiBlendTargetType.Type.GetChildInfo("nurbsCurve");
            multiBlendTargetType.nurbsChild = multiBlendTargetType.Type.GetChildInfo("nurbs");
            multiBlendTargetType.orientationConstraintChild = multiBlendTargetType.Type.GetChildInfo("orientationConstraint");
            multiBlendTargetType.parentConstraintChild = multiBlendTargetType.Type.GetChildInfo("parentConstraint");
            multiBlendTargetType.primitiveChild = multiBlendTargetType.Type.GetChildInfo("primitive");
            multiBlendTargetType.referenceChild = multiBlendTargetType.Type.GetChildInfo("reference");
            multiBlendTargetType.rigidBodyChild = multiBlendTargetType.Type.GetChildInfo("rigidBody");
            multiBlendTargetType.scaleConstraintChild = multiBlendTargetType.Type.GetChildInfo("scaleConstraint");
            multiBlendTargetType.springConstraintChild = multiBlendTargetType.Type.GetChildInfo("springConstraint");
            multiBlendTargetType.translationConstraintChild = multiBlendTargetType.Type.GetChildInfo("translationConstraint");
            multiBlendTargetType.animclipChild = multiBlendTargetType.Type.GetChildInfo("animclip");
            multiBlendTargetType.blendChild = multiBlendTargetType.Type.GetChildInfo("blend");
            multiBlendTargetType.blendshapeControllerChild = multiBlendTargetType.Type.GetChildInfo("blendshapeController");
            multiBlendTargetType.cgshaderChild = multiBlendTargetType.Type.GetChildInfo("cgshader");
            multiBlendTargetType.deformerChild = multiBlendTargetType.Type.GetChildInfo("deformer");
            multiBlendTargetType.expressionChild = multiBlendTargetType.Type.GetChildInfo("expression");
            multiBlendTargetType.imageChild = multiBlendTargetType.Type.GetChildInfo("image");
            multiBlendTargetType.materialChild = multiBlendTargetType.Type.GetChildInfo("material");
            multiBlendTargetType.motionPathChild = multiBlendTargetType.Type.GetChildInfo("motionPath");
            multiBlendTargetType.objsetChild = multiBlendTargetType.Type.GetChildInfo("objset");
            multiBlendTargetType.pmdataATGChild = multiBlendTargetType.Type.GetChildInfo("pmdataATG");
            multiBlendTargetType.poseChild = multiBlendTargetType.Type.GetChildInfo("pose");
            multiBlendTargetType.sceneChild = multiBlendTargetType.Type.GetChildInfo("scene");
            multiBlendTargetType.shaderChild = multiBlendTargetType.Type.GetChildInfo("shader");
            multiBlendTargetType.textureChild = multiBlendTargetType.Type.GetChildInfo("texture");
            multiBlendTargetType.blendshapeChild = multiBlendTargetType.Type.GetChildInfo("blendshape");
            multiBlendTargetType.skinChild = multiBlendTargetType.Type.GetChildInfo("skin");
            multiBlendTargetType.customDataChild = multiBlendTargetType.Type.GetChildInfo("customData");

            multiBlendTargetType_blendStage.Type = typeCollection.GetNodeType("multiBlendTargetType_blendStage");
            multiBlendTargetType_blendStage.interpolantAttribute = multiBlendTargetType_blendStage.Type.GetAttributeInfo("interpolant");
            multiBlendTargetType_blendStage.animChannelChild = multiBlendTargetType_blendStage.Type.GetChildInfo("animChannel");
            multiBlendTargetType_blendStage.animDiscontinuitiesChild = multiBlendTargetType_blendStage.Type.GetChildInfo("animDiscontinuities");
            multiBlendTargetType_blendStage.animChild = multiBlendTargetType_blendStage.Type.GetChildInfo("anim");
            multiBlendTargetType_blendStage.aimConstraintChild = multiBlendTargetType_blendStage.Type.GetChildInfo("aimConstraint");
            multiBlendTargetType_blendStage.atgilocatorChild = multiBlendTargetType_blendStage.Type.GetChildInfo("atgilocator");
            multiBlendTargetType_blendStage.blendtargetChild = multiBlendTargetType_blendStage.Type.GetChildInfo("blendtarget");
            multiBlendTargetType_blendStage.cameraChild = multiBlendTargetType_blendStage.Type.GetChildInfo("camera");
            multiBlendTargetType_blendStage.constraintChild = multiBlendTargetType_blendStage.Type.GetChildInfo("constraint");
            multiBlendTargetType_blendStage.clusterChild = multiBlendTargetType_blendStage.Type.GetChildInfo("cluster");
            multiBlendTargetType_blendStage.dynamicTypeChild = multiBlendTargetType_blendStage.Type.GetChildInfo("dynamicType");
            multiBlendTargetType_blendStage.instanceChild = multiBlendTargetType_blendStage.Type.GetChildInfo("instance");
            multiBlendTargetType_blendStage.jointChild = multiBlendTargetType_blendStage.Type.GetChildInfo("joint");
            multiBlendTargetType_blendStage.lightChild = multiBlendTargetType_blendStage.Type.GetChildInfo("light");
            multiBlendTargetType_blendStage.locatorChild = multiBlendTargetType_blendStage.Type.GetChildInfo("locator");
            multiBlendTargetType_blendStage.lodgroupChild = multiBlendTargetType_blendStage.Type.GetChildInfo("lodgroup");
            multiBlendTargetType_blendStage.meshChild = multiBlendTargetType_blendStage.Type.GetChildInfo("mesh");
            multiBlendTargetType_blendStage.multiBlendTargetChild = multiBlendTargetType_blendStage.Type.GetChildInfo("multiBlendTarget");
            multiBlendTargetType_blendStage.nodeChild = multiBlendTargetType_blendStage.Type.GetChildInfo("node");
            multiBlendTargetType_blendStage.nurbsCurveChild = multiBlendTargetType_blendStage.Type.GetChildInfo("nurbsCurve");
            multiBlendTargetType_blendStage.nurbsChild = multiBlendTargetType_blendStage.Type.GetChildInfo("nurbs");
            multiBlendTargetType_blendStage.orientationConstraintChild = multiBlendTargetType_blendStage.Type.GetChildInfo("orientationConstraint");
            multiBlendTargetType_blendStage.parentConstraintChild = multiBlendTargetType_blendStage.Type.GetChildInfo("parentConstraint");
            multiBlendTargetType_blendStage.primitiveChild = multiBlendTargetType_blendStage.Type.GetChildInfo("primitive");
            multiBlendTargetType_blendStage.referenceChild = multiBlendTargetType_blendStage.Type.GetChildInfo("reference");
            multiBlendTargetType_blendStage.rigidBodyChild = multiBlendTargetType_blendStage.Type.GetChildInfo("rigidBody");
            multiBlendTargetType_blendStage.scaleConstraintChild = multiBlendTargetType_blendStage.Type.GetChildInfo("scaleConstraint");
            multiBlendTargetType_blendStage.springConstraintChild = multiBlendTargetType_blendStage.Type.GetChildInfo("springConstraint");
            multiBlendTargetType_blendStage.translationConstraintChild = multiBlendTargetType_blendStage.Type.GetChildInfo("translationConstraint");
            multiBlendTargetType_blendStage.animclipChild = multiBlendTargetType_blendStage.Type.GetChildInfo("animclip");
            multiBlendTargetType_blendStage.blendChild = multiBlendTargetType_blendStage.Type.GetChildInfo("blend");
            multiBlendTargetType_blendStage.blendshapeControllerChild = multiBlendTargetType_blendStage.Type.GetChildInfo("blendshapeController");
            multiBlendTargetType_blendStage.cgshaderChild = multiBlendTargetType_blendStage.Type.GetChildInfo("cgshader");
            multiBlendTargetType_blendStage.deformerChild = multiBlendTargetType_blendStage.Type.GetChildInfo("deformer");
            multiBlendTargetType_blendStage.expressionChild = multiBlendTargetType_blendStage.Type.GetChildInfo("expression");
            multiBlendTargetType_blendStage.imageChild = multiBlendTargetType_blendStage.Type.GetChildInfo("image");
            multiBlendTargetType_blendStage.materialChild = multiBlendTargetType_blendStage.Type.GetChildInfo("material");
            multiBlendTargetType_blendStage.motionPathChild = multiBlendTargetType_blendStage.Type.GetChildInfo("motionPath");
            multiBlendTargetType_blendStage.objsetChild = multiBlendTargetType_blendStage.Type.GetChildInfo("objset");
            multiBlendTargetType_blendStage.pmdataATGChild = multiBlendTargetType_blendStage.Type.GetChildInfo("pmdataATG");
            multiBlendTargetType_blendStage.poseChild = multiBlendTargetType_blendStage.Type.GetChildInfo("pose");
            multiBlendTargetType_blendStage.sceneChild = multiBlendTargetType_blendStage.Type.GetChildInfo("scene");
            multiBlendTargetType_blendStage.shaderChild = multiBlendTargetType_blendStage.Type.GetChildInfo("shader");
            multiBlendTargetType_blendStage.textureChild = multiBlendTargetType_blendStage.Type.GetChildInfo("texture");
            multiBlendTargetType_blendStage.blendshapeChild = multiBlendTargetType_blendStage.Type.GetChildInfo("blendshape");
            multiBlendTargetType_blendStage.skinChild = multiBlendTargetType_blendStage.Type.GetChildInfo("skin");
            multiBlendTargetType_blendStage.customDataChild = multiBlendTargetType_blendStage.Type.GetChildInfo("customData");

            nurbsCurveType.Type = typeCollection.GetNodeType("nurbsCurveType");
            nurbsCurveType.nameAttribute = nurbsCurveType.Type.GetAttributeInfo("name");
            nurbsCurveType.degreeAttribute = nurbsCurveType.Type.GetAttributeInfo("degree");
            nurbsCurveType.formAttribute = nurbsCurveType.Type.GetAttributeInfo("form");
            nurbsCurveType.pointsChild = nurbsCurveType.Type.GetChildInfo("points");
            nurbsCurveType.knotsChild = nurbsCurveType.Type.GetChildInfo("knots");
            nurbsCurveType.animChannelChild = nurbsCurveType.Type.GetChildInfo("animChannel");
            nurbsCurveType.animDiscontinuitiesChild = nurbsCurveType.Type.GetChildInfo("animDiscontinuities");
            nurbsCurveType.animChild = nurbsCurveType.Type.GetChildInfo("anim");
            nurbsCurveType.aimConstraintChild = nurbsCurveType.Type.GetChildInfo("aimConstraint");
            nurbsCurveType.atgilocatorChild = nurbsCurveType.Type.GetChildInfo("atgilocator");
            nurbsCurveType.blendtargetChild = nurbsCurveType.Type.GetChildInfo("blendtarget");
            nurbsCurveType.cameraChild = nurbsCurveType.Type.GetChildInfo("camera");
            nurbsCurveType.constraintChild = nurbsCurveType.Type.GetChildInfo("constraint");
            nurbsCurveType.clusterChild = nurbsCurveType.Type.GetChildInfo("cluster");
            nurbsCurveType.dynamicTypeChild = nurbsCurveType.Type.GetChildInfo("dynamicType");
            nurbsCurveType.instanceChild = nurbsCurveType.Type.GetChildInfo("instance");
            nurbsCurveType.jointChild = nurbsCurveType.Type.GetChildInfo("joint");
            nurbsCurveType.lightChild = nurbsCurveType.Type.GetChildInfo("light");
            nurbsCurveType.locatorChild = nurbsCurveType.Type.GetChildInfo("locator");
            nurbsCurveType.lodgroupChild = nurbsCurveType.Type.GetChildInfo("lodgroup");
            nurbsCurveType.meshChild = nurbsCurveType.Type.GetChildInfo("mesh");
            nurbsCurveType.multiBlendTargetChild = nurbsCurveType.Type.GetChildInfo("multiBlendTarget");
            nurbsCurveType.nodeChild = nurbsCurveType.Type.GetChildInfo("node");
            nurbsCurveType.nurbsCurveChild = nurbsCurveType.Type.GetChildInfo("nurbsCurve");
            nurbsCurveType.nurbsChild = nurbsCurveType.Type.GetChildInfo("nurbs");
            nurbsCurveType.orientationConstraintChild = nurbsCurveType.Type.GetChildInfo("orientationConstraint");
            nurbsCurveType.parentConstraintChild = nurbsCurveType.Type.GetChildInfo("parentConstraint");
            nurbsCurveType.primitiveChild = nurbsCurveType.Type.GetChildInfo("primitive");
            nurbsCurveType.referenceChild = nurbsCurveType.Type.GetChildInfo("reference");
            nurbsCurveType.rigidBodyChild = nurbsCurveType.Type.GetChildInfo("rigidBody");
            nurbsCurveType.scaleConstraintChild = nurbsCurveType.Type.GetChildInfo("scaleConstraint");
            nurbsCurveType.springConstraintChild = nurbsCurveType.Type.GetChildInfo("springConstraint");
            nurbsCurveType.translationConstraintChild = nurbsCurveType.Type.GetChildInfo("translationConstraint");
            nurbsCurveType.animclipChild = nurbsCurveType.Type.GetChildInfo("animclip");
            nurbsCurveType.blendChild = nurbsCurveType.Type.GetChildInfo("blend");
            nurbsCurveType.blendshapeControllerChild = nurbsCurveType.Type.GetChildInfo("blendshapeController");
            nurbsCurveType.cgshaderChild = nurbsCurveType.Type.GetChildInfo("cgshader");
            nurbsCurveType.deformerChild = nurbsCurveType.Type.GetChildInfo("deformer");
            nurbsCurveType.expressionChild = nurbsCurveType.Type.GetChildInfo("expression");
            nurbsCurveType.imageChild = nurbsCurveType.Type.GetChildInfo("image");
            nurbsCurveType.materialChild = nurbsCurveType.Type.GetChildInfo("material");
            nurbsCurveType.motionPathChild = nurbsCurveType.Type.GetChildInfo("motionPath");
            nurbsCurveType.objsetChild = nurbsCurveType.Type.GetChildInfo("objset");
            nurbsCurveType.pmdataATGChild = nurbsCurveType.Type.GetChildInfo("pmdataATG");
            nurbsCurveType.poseChild = nurbsCurveType.Type.GetChildInfo("pose");
            nurbsCurveType.sceneChild = nurbsCurveType.Type.GetChildInfo("scene");
            nurbsCurveType.shaderChild = nurbsCurveType.Type.GetChildInfo("shader");
            nurbsCurveType.textureChild = nurbsCurveType.Type.GetChildInfo("texture");
            nurbsCurveType.blendshapeChild = nurbsCurveType.Type.GetChildInfo("blendshape");
            nurbsCurveType.skinChild = nurbsCurveType.Type.GetChildInfo("skin");
            nurbsCurveType.customDataChild = nurbsCurveType.Type.GetChildInfo("customData");

            nurbsCurveType_points.Type = typeCollection.GetNodeType("nurbsCurveType_points");
            nurbsCurveType_points.Attribute = nurbsCurveType_points.Type.GetAttributeInfo("");
            nurbsCurveType_points.countAttribute = nurbsCurveType_points.Type.GetAttributeInfo("count");

            nurbsCurveType_knots.Type = typeCollection.GetNodeType("nurbsCurveType_knots");
            nurbsCurveType_knots.Attribute = nurbsCurveType_knots.Type.GetAttributeInfo("");
            nurbsCurveType_knots.countAttribute = nurbsCurveType_knots.Type.GetAttributeInfo("count");

            nurbsSurfaceType.Type = typeCollection.GetNodeType("nurbsSurfaceType");
            nurbsSurfaceType.nameAttribute = nurbsSurfaceType.Type.GetAttributeInfo("name");
            nurbsSurfaceType.surfaceChild = nurbsSurfaceType.Type.GetChildInfo("surface");
            nurbsSurfaceType.animChannelChild = nurbsSurfaceType.Type.GetChildInfo("animChannel");
            nurbsSurfaceType.animDiscontinuitiesChild = nurbsSurfaceType.Type.GetChildInfo("animDiscontinuities");
            nurbsSurfaceType.animChild = nurbsSurfaceType.Type.GetChildInfo("anim");
            nurbsSurfaceType.aimConstraintChild = nurbsSurfaceType.Type.GetChildInfo("aimConstraint");
            nurbsSurfaceType.atgilocatorChild = nurbsSurfaceType.Type.GetChildInfo("atgilocator");
            nurbsSurfaceType.blendtargetChild = nurbsSurfaceType.Type.GetChildInfo("blendtarget");
            nurbsSurfaceType.cameraChild = nurbsSurfaceType.Type.GetChildInfo("camera");
            nurbsSurfaceType.constraintChild = nurbsSurfaceType.Type.GetChildInfo("constraint");
            nurbsSurfaceType.clusterChild = nurbsSurfaceType.Type.GetChildInfo("cluster");
            nurbsSurfaceType.dynamicTypeChild = nurbsSurfaceType.Type.GetChildInfo("dynamicType");
            nurbsSurfaceType.instanceChild = nurbsSurfaceType.Type.GetChildInfo("instance");
            nurbsSurfaceType.jointChild = nurbsSurfaceType.Type.GetChildInfo("joint");
            nurbsSurfaceType.lightChild = nurbsSurfaceType.Type.GetChildInfo("light");
            nurbsSurfaceType.locatorChild = nurbsSurfaceType.Type.GetChildInfo("locator");
            nurbsSurfaceType.lodgroupChild = nurbsSurfaceType.Type.GetChildInfo("lodgroup");
            nurbsSurfaceType.meshChild = nurbsSurfaceType.Type.GetChildInfo("mesh");
            nurbsSurfaceType.multiBlendTargetChild = nurbsSurfaceType.Type.GetChildInfo("multiBlendTarget");
            nurbsSurfaceType.nodeChild = nurbsSurfaceType.Type.GetChildInfo("node");
            nurbsSurfaceType.nurbsCurveChild = nurbsSurfaceType.Type.GetChildInfo("nurbsCurve");
            nurbsSurfaceType.nurbsChild = nurbsSurfaceType.Type.GetChildInfo("nurbs");
            nurbsSurfaceType.orientationConstraintChild = nurbsSurfaceType.Type.GetChildInfo("orientationConstraint");
            nurbsSurfaceType.parentConstraintChild = nurbsSurfaceType.Type.GetChildInfo("parentConstraint");
            nurbsSurfaceType.primitiveChild = nurbsSurfaceType.Type.GetChildInfo("primitive");
            nurbsSurfaceType.referenceChild = nurbsSurfaceType.Type.GetChildInfo("reference");
            nurbsSurfaceType.rigidBodyChild = nurbsSurfaceType.Type.GetChildInfo("rigidBody");
            nurbsSurfaceType.scaleConstraintChild = nurbsSurfaceType.Type.GetChildInfo("scaleConstraint");
            nurbsSurfaceType.springConstraintChild = nurbsSurfaceType.Type.GetChildInfo("springConstraint");
            nurbsSurfaceType.translationConstraintChild = nurbsSurfaceType.Type.GetChildInfo("translationConstraint");
            nurbsSurfaceType.animclipChild = nurbsSurfaceType.Type.GetChildInfo("animclip");
            nurbsSurfaceType.blendChild = nurbsSurfaceType.Type.GetChildInfo("blend");
            nurbsSurfaceType.blendshapeControllerChild = nurbsSurfaceType.Type.GetChildInfo("blendshapeController");
            nurbsSurfaceType.cgshaderChild = nurbsSurfaceType.Type.GetChildInfo("cgshader");
            nurbsSurfaceType.deformerChild = nurbsSurfaceType.Type.GetChildInfo("deformer");
            nurbsSurfaceType.expressionChild = nurbsSurfaceType.Type.GetChildInfo("expression");
            nurbsSurfaceType.imageChild = nurbsSurfaceType.Type.GetChildInfo("image");
            nurbsSurfaceType.materialChild = nurbsSurfaceType.Type.GetChildInfo("material");
            nurbsSurfaceType.motionPathChild = nurbsSurfaceType.Type.GetChildInfo("motionPath");
            nurbsSurfaceType.objsetChild = nurbsSurfaceType.Type.GetChildInfo("objset");
            nurbsSurfaceType.pmdataATGChild = nurbsSurfaceType.Type.GetChildInfo("pmdataATG");
            nurbsSurfaceType.poseChild = nurbsSurfaceType.Type.GetChildInfo("pose");
            nurbsSurfaceType.sceneChild = nurbsSurfaceType.Type.GetChildInfo("scene");
            nurbsSurfaceType.shaderChild = nurbsSurfaceType.Type.GetChildInfo("shader");
            nurbsSurfaceType.textureChild = nurbsSurfaceType.Type.GetChildInfo("texture");
            nurbsSurfaceType.blendshapeChild = nurbsSurfaceType.Type.GetChildInfo("blendshape");
            nurbsSurfaceType.skinChild = nurbsSurfaceType.Type.GetChildInfo("skin");
            nurbsSurfaceType.customDataChild = nurbsSurfaceType.Type.GetChildInfo("customData");

            nurbsSurfaceType_surface.Type = typeCollection.GetNodeType("nurbsSurfaceType_surface");
            nurbsSurfaceType_surface.uOrderAttribute = nurbsSurfaceType_surface.Type.GetAttributeInfo("uOrder");
            nurbsSurfaceType_surface.vOrderAttribute = nurbsSurfaceType_surface.Type.GetAttributeInfo("vOrder");
            nurbsSurfaceType_surface.uMinAttribute = nurbsSurfaceType_surface.Type.GetAttributeInfo("uMin");
            nurbsSurfaceType_surface.uMaxAttribute = nurbsSurfaceType_surface.Type.GetAttributeInfo("uMax");
            nurbsSurfaceType_surface.vMinAttribute = nurbsSurfaceType_surface.Type.GetAttributeInfo("vMin");
            nurbsSurfaceType_surface.vMaxAttribute = nurbsSurfaceType_surface.Type.GetAttributeInfo("vMax");
            nurbsSurfaceType_surface.controlVerticesChild = nurbsSurfaceType_surface.Type.GetChildInfo("controlVertices");
            nurbsSurfaceType_surface.knotsInUChild = nurbsSurfaceType_surface.Type.GetChildInfo("knotsInU");
            nurbsSurfaceType_surface.knotsInVChild = nurbsSurfaceType_surface.Type.GetChildInfo("knotsInV");

            surface_controlVertices.Type = typeCollection.GetNodeType("surface_controlVertices");
            surface_controlVertices.Attribute = surface_controlVertices.Type.GetAttributeInfo("");
            surface_controlVertices.numCVinUAttribute = surface_controlVertices.Type.GetAttributeInfo("numCVinU");
            surface_controlVertices.numCVinVAttribute = surface_controlVertices.Type.GetAttributeInfo("numCVinV");

            surface_knotsInU.Type = typeCollection.GetNodeType("surface_knotsInU");
            surface_knotsInU.Attribute = surface_knotsInU.Type.GetAttributeInfo("");
            surface_knotsInU.numKnotsInUAttribute = surface_knotsInU.Type.GetAttributeInfo("numKnotsInU");

            surface_knotsInV.Type = typeCollection.GetNodeType("surface_knotsInV");
            surface_knotsInV.Attribute = surface_knotsInV.Type.GetAttributeInfo("");
            surface_knotsInV.numKnotsInVAttribute = surface_knotsInV.Type.GetAttributeInfo("numKnotsInV");

            orientConstraintType.Type = typeCollection.GetNodeType("orientConstraintType");
            orientConstraintType.nameAttribute = orientConstraintType.Type.GetAttributeInfo("name");
            orientConstraintType.constrainAttribute = orientConstraintType.Type.GetAttributeInfo("constrain");
            orientConstraintType.orientInterpolationAttribute = orientConstraintType.Type.GetAttributeInfo("orientInterpolation");
            orientConstraintType.targetChild = orientConstraintType.Type.GetChildInfo("target");
            orientConstraintType.animChannelChild = orientConstraintType.Type.GetChildInfo("animChannel");
            orientConstraintType.animDiscontinuitiesChild = orientConstraintType.Type.GetChildInfo("animDiscontinuities");
            orientConstraintType.animChild = orientConstraintType.Type.GetChildInfo("anim");
            orientConstraintType.aimConstraintChild = orientConstraintType.Type.GetChildInfo("aimConstraint");
            orientConstraintType.atgilocatorChild = orientConstraintType.Type.GetChildInfo("atgilocator");
            orientConstraintType.blendtargetChild = orientConstraintType.Type.GetChildInfo("blendtarget");
            orientConstraintType.cameraChild = orientConstraintType.Type.GetChildInfo("camera");
            orientConstraintType.constraintChild = orientConstraintType.Type.GetChildInfo("constraint");
            orientConstraintType.clusterChild = orientConstraintType.Type.GetChildInfo("cluster");
            orientConstraintType.dynamicTypeChild = orientConstraintType.Type.GetChildInfo("dynamicType");
            orientConstraintType.instanceChild = orientConstraintType.Type.GetChildInfo("instance");
            orientConstraintType.jointChild = orientConstraintType.Type.GetChildInfo("joint");
            orientConstraintType.lightChild = orientConstraintType.Type.GetChildInfo("light");
            orientConstraintType.locatorChild = orientConstraintType.Type.GetChildInfo("locator");
            orientConstraintType.lodgroupChild = orientConstraintType.Type.GetChildInfo("lodgroup");
            orientConstraintType.meshChild = orientConstraintType.Type.GetChildInfo("mesh");
            orientConstraintType.multiBlendTargetChild = orientConstraintType.Type.GetChildInfo("multiBlendTarget");
            orientConstraintType.nodeChild = orientConstraintType.Type.GetChildInfo("node");
            orientConstraintType.nurbsCurveChild = orientConstraintType.Type.GetChildInfo("nurbsCurve");
            orientConstraintType.nurbsChild = orientConstraintType.Type.GetChildInfo("nurbs");
            orientConstraintType.orientationConstraintChild = orientConstraintType.Type.GetChildInfo("orientationConstraint");
            orientConstraintType.parentConstraintChild = orientConstraintType.Type.GetChildInfo("parentConstraint");
            orientConstraintType.primitiveChild = orientConstraintType.Type.GetChildInfo("primitive");
            orientConstraintType.referenceChild = orientConstraintType.Type.GetChildInfo("reference");
            orientConstraintType.rigidBodyChild = orientConstraintType.Type.GetChildInfo("rigidBody");
            orientConstraintType.scaleConstraintChild = orientConstraintType.Type.GetChildInfo("scaleConstraint");
            orientConstraintType.springConstraintChild = orientConstraintType.Type.GetChildInfo("springConstraint");
            orientConstraintType.translationConstraintChild = orientConstraintType.Type.GetChildInfo("translationConstraint");
            orientConstraintType.animclipChild = orientConstraintType.Type.GetChildInfo("animclip");
            orientConstraintType.blendChild = orientConstraintType.Type.GetChildInfo("blend");
            orientConstraintType.blendshapeControllerChild = orientConstraintType.Type.GetChildInfo("blendshapeController");
            orientConstraintType.cgshaderChild = orientConstraintType.Type.GetChildInfo("cgshader");
            orientConstraintType.deformerChild = orientConstraintType.Type.GetChildInfo("deformer");
            orientConstraintType.expressionChild = orientConstraintType.Type.GetChildInfo("expression");
            orientConstraintType.imageChild = orientConstraintType.Type.GetChildInfo("image");
            orientConstraintType.materialChild = orientConstraintType.Type.GetChildInfo("material");
            orientConstraintType.motionPathChild = orientConstraintType.Type.GetChildInfo("motionPath");
            orientConstraintType.objsetChild = orientConstraintType.Type.GetChildInfo("objset");
            orientConstraintType.pmdataATGChild = orientConstraintType.Type.GetChildInfo("pmdataATG");
            orientConstraintType.poseChild = orientConstraintType.Type.GetChildInfo("pose");
            orientConstraintType.sceneChild = orientConstraintType.Type.GetChildInfo("scene");
            orientConstraintType.shaderChild = orientConstraintType.Type.GetChildInfo("shader");
            orientConstraintType.textureChild = orientConstraintType.Type.GetChildInfo("texture");
            orientConstraintType.blendshapeChild = orientConstraintType.Type.GetChildInfo("blendshape");
            orientConstraintType.skinChild = orientConstraintType.Type.GetChildInfo("skin");
            orientConstraintType.customDataChild = orientConstraintType.Type.GetChildInfo("customData");
            orientConstraintType.rotEulChild = orientConstraintType.Type.GetChildInfo("rotEul");

            parentConstraintType.Type = typeCollection.GetNodeType("parentConstraintType");
            parentConstraintType.nameAttribute = parentConstraintType.Type.GetAttributeInfo("name");
            parentConstraintType.constrainAttribute = parentConstraintType.Type.GetAttributeInfo("constrain");
            parentConstraintType.offsetChild = parentConstraintType.Type.GetChildInfo("offset");
            parentConstraintType.targetChild = parentConstraintType.Type.GetChildInfo("target");
            parentConstraintType.animChannelChild = parentConstraintType.Type.GetChildInfo("animChannel");
            parentConstraintType.animDiscontinuitiesChild = parentConstraintType.Type.GetChildInfo("animDiscontinuities");
            parentConstraintType.animChild = parentConstraintType.Type.GetChildInfo("anim");
            parentConstraintType.aimConstraintChild = parentConstraintType.Type.GetChildInfo("aimConstraint");
            parentConstraintType.atgilocatorChild = parentConstraintType.Type.GetChildInfo("atgilocator");
            parentConstraintType.blendtargetChild = parentConstraintType.Type.GetChildInfo("blendtarget");
            parentConstraintType.cameraChild = parentConstraintType.Type.GetChildInfo("camera");
            parentConstraintType.constraintChild = parentConstraintType.Type.GetChildInfo("constraint");
            parentConstraintType.clusterChild = parentConstraintType.Type.GetChildInfo("cluster");
            parentConstraintType.dynamicTypeChild = parentConstraintType.Type.GetChildInfo("dynamicType");
            parentConstraintType.instanceChild = parentConstraintType.Type.GetChildInfo("instance");
            parentConstraintType.jointChild = parentConstraintType.Type.GetChildInfo("joint");
            parentConstraintType.lightChild = parentConstraintType.Type.GetChildInfo("light");
            parentConstraintType.locatorChild = parentConstraintType.Type.GetChildInfo("locator");
            parentConstraintType.lodgroupChild = parentConstraintType.Type.GetChildInfo("lodgroup");
            parentConstraintType.meshChild = parentConstraintType.Type.GetChildInfo("mesh");
            parentConstraintType.multiBlendTargetChild = parentConstraintType.Type.GetChildInfo("multiBlendTarget");
            parentConstraintType.nodeChild = parentConstraintType.Type.GetChildInfo("node");
            parentConstraintType.nurbsCurveChild = parentConstraintType.Type.GetChildInfo("nurbsCurve");
            parentConstraintType.nurbsChild = parentConstraintType.Type.GetChildInfo("nurbs");
            parentConstraintType.orientationConstraintChild = parentConstraintType.Type.GetChildInfo("orientationConstraint");
            parentConstraintType.parentConstraintChild = parentConstraintType.Type.GetChildInfo("parentConstraint");
            parentConstraintType.primitiveChild = parentConstraintType.Type.GetChildInfo("primitive");
            parentConstraintType.referenceChild = parentConstraintType.Type.GetChildInfo("reference");
            parentConstraintType.rigidBodyChild = parentConstraintType.Type.GetChildInfo("rigidBody");
            parentConstraintType.scaleConstraintChild = parentConstraintType.Type.GetChildInfo("scaleConstraint");
            parentConstraintType.springConstraintChild = parentConstraintType.Type.GetChildInfo("springConstraint");
            parentConstraintType.translationConstraintChild = parentConstraintType.Type.GetChildInfo("translationConstraint");
            parentConstraintType.animclipChild = parentConstraintType.Type.GetChildInfo("animclip");
            parentConstraintType.blendChild = parentConstraintType.Type.GetChildInfo("blend");
            parentConstraintType.blendshapeControllerChild = parentConstraintType.Type.GetChildInfo("blendshapeController");
            parentConstraintType.cgshaderChild = parentConstraintType.Type.GetChildInfo("cgshader");
            parentConstraintType.deformerChild = parentConstraintType.Type.GetChildInfo("deformer");
            parentConstraintType.expressionChild = parentConstraintType.Type.GetChildInfo("expression");
            parentConstraintType.imageChild = parentConstraintType.Type.GetChildInfo("image");
            parentConstraintType.materialChild = parentConstraintType.Type.GetChildInfo("material");
            parentConstraintType.motionPathChild = parentConstraintType.Type.GetChildInfo("motionPath");
            parentConstraintType.objsetChild = parentConstraintType.Type.GetChildInfo("objset");
            parentConstraintType.pmdataATGChild = parentConstraintType.Type.GetChildInfo("pmdataATG");
            parentConstraintType.poseChild = parentConstraintType.Type.GetChildInfo("pose");
            parentConstraintType.sceneChild = parentConstraintType.Type.GetChildInfo("scene");
            parentConstraintType.shaderChild = parentConstraintType.Type.GetChildInfo("shader");
            parentConstraintType.textureChild = parentConstraintType.Type.GetChildInfo("texture");
            parentConstraintType.blendshapeChild = parentConstraintType.Type.GetChildInfo("blendshape");
            parentConstraintType.skinChild = parentConstraintType.Type.GetChildInfo("skin");
            parentConstraintType.customDataChild = parentConstraintType.Type.GetChildInfo("customData");

            parentConstraintType_offset.Type = typeCollection.GetNodeType("parentConstraintType_offset");
            parentConstraintType_offset.translationAttribute = parentConstraintType_offset.Type.GetAttributeInfo("translation");
            parentConstraintType_offset.rotEulChild = parentConstraintType_offset.Type.GetChildInfo("rotEul");

            primitiveShapeType.Type = typeCollection.GetNodeType("primitiveShapeType");
            primitiveShapeType.nameAttribute = primitiveShapeType.Type.GetAttributeInfo("name");
            primitiveShapeType.shaderAttribute = primitiveShapeType.Type.GetAttributeInfo("shader");
            primitiveShapeType.cubeChild = primitiveShapeType.Type.GetChildInfo("cube");
            primitiveShapeType.cylinderChild = primitiveShapeType.Type.GetChildInfo("cylinder");
            primitiveShapeType.sphereChild = primitiveShapeType.Type.GetChildInfo("sphere");
            primitiveShapeType.animChannelChild = primitiveShapeType.Type.GetChildInfo("animChannel");
            primitiveShapeType.animDiscontinuitiesChild = primitiveShapeType.Type.GetChildInfo("animDiscontinuities");
            primitiveShapeType.animChild = primitiveShapeType.Type.GetChildInfo("anim");
            primitiveShapeType.aimConstraintChild = primitiveShapeType.Type.GetChildInfo("aimConstraint");
            primitiveShapeType.atgilocatorChild = primitiveShapeType.Type.GetChildInfo("atgilocator");
            primitiveShapeType.blendtargetChild = primitiveShapeType.Type.GetChildInfo("blendtarget");
            primitiveShapeType.cameraChild = primitiveShapeType.Type.GetChildInfo("camera");
            primitiveShapeType.constraintChild = primitiveShapeType.Type.GetChildInfo("constraint");
            primitiveShapeType.clusterChild = primitiveShapeType.Type.GetChildInfo("cluster");
            primitiveShapeType.dynamicTypeChild = primitiveShapeType.Type.GetChildInfo("dynamicType");
            primitiveShapeType.instanceChild = primitiveShapeType.Type.GetChildInfo("instance");
            primitiveShapeType.jointChild = primitiveShapeType.Type.GetChildInfo("joint");
            primitiveShapeType.lightChild = primitiveShapeType.Type.GetChildInfo("light");
            primitiveShapeType.locatorChild = primitiveShapeType.Type.GetChildInfo("locator");
            primitiveShapeType.lodgroupChild = primitiveShapeType.Type.GetChildInfo("lodgroup");
            primitiveShapeType.meshChild = primitiveShapeType.Type.GetChildInfo("mesh");
            primitiveShapeType.multiBlendTargetChild = primitiveShapeType.Type.GetChildInfo("multiBlendTarget");
            primitiveShapeType.nodeChild = primitiveShapeType.Type.GetChildInfo("node");
            primitiveShapeType.nurbsCurveChild = primitiveShapeType.Type.GetChildInfo("nurbsCurve");
            primitiveShapeType.nurbsChild = primitiveShapeType.Type.GetChildInfo("nurbs");
            primitiveShapeType.orientationConstraintChild = primitiveShapeType.Type.GetChildInfo("orientationConstraint");
            primitiveShapeType.parentConstraintChild = primitiveShapeType.Type.GetChildInfo("parentConstraint");
            primitiveShapeType.primitiveChild = primitiveShapeType.Type.GetChildInfo("primitive");
            primitiveShapeType.referenceChild = primitiveShapeType.Type.GetChildInfo("reference");
            primitiveShapeType.rigidBodyChild = primitiveShapeType.Type.GetChildInfo("rigidBody");
            primitiveShapeType.scaleConstraintChild = primitiveShapeType.Type.GetChildInfo("scaleConstraint");
            primitiveShapeType.springConstraintChild = primitiveShapeType.Type.GetChildInfo("springConstraint");
            primitiveShapeType.translationConstraintChild = primitiveShapeType.Type.GetChildInfo("translationConstraint");
            primitiveShapeType.animclipChild = primitiveShapeType.Type.GetChildInfo("animclip");
            primitiveShapeType.blendChild = primitiveShapeType.Type.GetChildInfo("blend");
            primitiveShapeType.blendshapeControllerChild = primitiveShapeType.Type.GetChildInfo("blendshapeController");
            primitiveShapeType.cgshaderChild = primitiveShapeType.Type.GetChildInfo("cgshader");
            primitiveShapeType.deformerChild = primitiveShapeType.Type.GetChildInfo("deformer");
            primitiveShapeType.expressionChild = primitiveShapeType.Type.GetChildInfo("expression");
            primitiveShapeType.imageChild = primitiveShapeType.Type.GetChildInfo("image");
            primitiveShapeType.materialChild = primitiveShapeType.Type.GetChildInfo("material");
            primitiveShapeType.motionPathChild = primitiveShapeType.Type.GetChildInfo("motionPath");
            primitiveShapeType.objsetChild = primitiveShapeType.Type.GetChildInfo("objset");
            primitiveShapeType.pmdataATGChild = primitiveShapeType.Type.GetChildInfo("pmdataATG");
            primitiveShapeType.poseChild = primitiveShapeType.Type.GetChildInfo("pose");
            primitiveShapeType.sceneChild = primitiveShapeType.Type.GetChildInfo("scene");
            primitiveShapeType.shaderChild = primitiveShapeType.Type.GetChildInfo("shader");
            primitiveShapeType.textureChild = primitiveShapeType.Type.GetChildInfo("texture");
            primitiveShapeType.blendshapeChild = primitiveShapeType.Type.GetChildInfo("blendshape");
            primitiveShapeType.skinChild = primitiveShapeType.Type.GetChildInfo("skin");
            primitiveShapeType.customDataChild = primitiveShapeType.Type.GetChildInfo("customData");

            primitiveShapeType_cube.Type = typeCollection.GetNodeType("primitiveShapeType_cube");
            primitiveShapeType_cube.widthAttribute = primitiveShapeType_cube.Type.GetAttributeInfo("width");
            primitiveShapeType_cube.heightAttribute = primitiveShapeType_cube.Type.GetAttributeInfo("height");
            primitiveShapeType_cube.depthAttribute = primitiveShapeType_cube.Type.GetAttributeInfo("depth");

            primitiveShapeType_cylinder.Type = typeCollection.GetNodeType("primitiveShapeType_cylinder");
            primitiveShapeType_cylinder.radiusAttribute = primitiveShapeType_cylinder.Type.GetAttributeInfo("radius");
            primitiveShapeType_cylinder.heightAttribute = primitiveShapeType_cylinder.Type.GetAttributeInfo("height");

            primitiveShapeType_sphere.Type = typeCollection.GetNodeType("primitiveShapeType_sphere");
            primitiveShapeType_sphere.radiusAttribute = primitiveShapeType_sphere.Type.GetAttributeInfo("radius");

            referenceType.Type = typeCollection.GetNodeType("referenceType");
            referenceType.nameAttribute = referenceType.Type.GetAttributeInfo("name");
            referenceType.uriAttribute = referenceType.Type.GetAttributeInfo("uri");
            referenceType.namespaceAttribute = referenceType.Type.GetAttributeInfo("namespace");
            referenceType.animChannelChild = referenceType.Type.GetChildInfo("animChannel");
            referenceType.animDiscontinuitiesChild = referenceType.Type.GetChildInfo("animDiscontinuities");
            referenceType.animChild = referenceType.Type.GetChildInfo("anim");
            referenceType.aimConstraintChild = referenceType.Type.GetChildInfo("aimConstraint");
            referenceType.atgilocatorChild = referenceType.Type.GetChildInfo("atgilocator");
            referenceType.blendtargetChild = referenceType.Type.GetChildInfo("blendtarget");
            referenceType.cameraChild = referenceType.Type.GetChildInfo("camera");
            referenceType.constraintChild = referenceType.Type.GetChildInfo("constraint");
            referenceType.clusterChild = referenceType.Type.GetChildInfo("cluster");
            referenceType.dynamicTypeChild = referenceType.Type.GetChildInfo("dynamicType");
            referenceType.instanceChild = referenceType.Type.GetChildInfo("instance");
            referenceType.jointChild = referenceType.Type.GetChildInfo("joint");
            referenceType.lightChild = referenceType.Type.GetChildInfo("light");
            referenceType.locatorChild = referenceType.Type.GetChildInfo("locator");
            referenceType.lodgroupChild = referenceType.Type.GetChildInfo("lodgroup");
            referenceType.meshChild = referenceType.Type.GetChildInfo("mesh");
            referenceType.multiBlendTargetChild = referenceType.Type.GetChildInfo("multiBlendTarget");
            referenceType.nodeChild = referenceType.Type.GetChildInfo("node");
            referenceType.nurbsCurveChild = referenceType.Type.GetChildInfo("nurbsCurve");
            referenceType.nurbsChild = referenceType.Type.GetChildInfo("nurbs");
            referenceType.orientationConstraintChild = referenceType.Type.GetChildInfo("orientationConstraint");
            referenceType.parentConstraintChild = referenceType.Type.GetChildInfo("parentConstraint");
            referenceType.primitiveChild = referenceType.Type.GetChildInfo("primitive");
            referenceType.referenceChild = referenceType.Type.GetChildInfo("reference");
            referenceType.rigidBodyChild = referenceType.Type.GetChildInfo("rigidBody");
            referenceType.scaleConstraintChild = referenceType.Type.GetChildInfo("scaleConstraint");
            referenceType.springConstraintChild = referenceType.Type.GetChildInfo("springConstraint");
            referenceType.translationConstraintChild = referenceType.Type.GetChildInfo("translationConstraint");
            referenceType.animclipChild = referenceType.Type.GetChildInfo("animclip");
            referenceType.blendChild = referenceType.Type.GetChildInfo("blend");
            referenceType.blendshapeControllerChild = referenceType.Type.GetChildInfo("blendshapeController");
            referenceType.cgshaderChild = referenceType.Type.GetChildInfo("cgshader");
            referenceType.deformerChild = referenceType.Type.GetChildInfo("deformer");
            referenceType.expressionChild = referenceType.Type.GetChildInfo("expression");
            referenceType.imageChild = referenceType.Type.GetChildInfo("image");
            referenceType.materialChild = referenceType.Type.GetChildInfo("material");
            referenceType.motionPathChild = referenceType.Type.GetChildInfo("motionPath");
            referenceType.objsetChild = referenceType.Type.GetChildInfo("objset");
            referenceType.pmdataATGChild = referenceType.Type.GetChildInfo("pmdataATG");
            referenceType.poseChild = referenceType.Type.GetChildInfo("pose");
            referenceType.sceneChild = referenceType.Type.GetChildInfo("scene");
            referenceType.shaderChild = referenceType.Type.GetChildInfo("shader");
            referenceType.textureChild = referenceType.Type.GetChildInfo("texture");
            referenceType.blendshapeChild = referenceType.Type.GetChildInfo("blendshape");
            referenceType.skinChild = referenceType.Type.GetChildInfo("skin");
            referenceType.customDataChild = referenceType.Type.GetChildInfo("customData");

            rigidBodyType.Type = typeCollection.GetNodeType("rigidBodyType");
            rigidBodyType.nameAttribute = rigidBodyType.Type.GetAttributeInfo("name");
            rigidBodyType.massAttribute = rigidBodyType.Type.GetAttributeInfo("mass");
            rigidBodyType.geometryAttribute = rigidBodyType.Type.GetAttributeInfo("geometry");
            rigidBodyType.animChannelChild = rigidBodyType.Type.GetChildInfo("animChannel");
            rigidBodyType.animDiscontinuitiesChild = rigidBodyType.Type.GetChildInfo("animDiscontinuities");
            rigidBodyType.animChild = rigidBodyType.Type.GetChildInfo("anim");
            rigidBodyType.aimConstraintChild = rigidBodyType.Type.GetChildInfo("aimConstraint");
            rigidBodyType.atgilocatorChild = rigidBodyType.Type.GetChildInfo("atgilocator");
            rigidBodyType.blendtargetChild = rigidBodyType.Type.GetChildInfo("blendtarget");
            rigidBodyType.cameraChild = rigidBodyType.Type.GetChildInfo("camera");
            rigidBodyType.constraintChild = rigidBodyType.Type.GetChildInfo("constraint");
            rigidBodyType.clusterChild = rigidBodyType.Type.GetChildInfo("cluster");
            rigidBodyType.dynamicTypeChild = rigidBodyType.Type.GetChildInfo("dynamicType");
            rigidBodyType.instanceChild = rigidBodyType.Type.GetChildInfo("instance");
            rigidBodyType.jointChild = rigidBodyType.Type.GetChildInfo("joint");
            rigidBodyType.lightChild = rigidBodyType.Type.GetChildInfo("light");
            rigidBodyType.locatorChild = rigidBodyType.Type.GetChildInfo("locator");
            rigidBodyType.lodgroupChild = rigidBodyType.Type.GetChildInfo("lodgroup");
            rigidBodyType.meshChild = rigidBodyType.Type.GetChildInfo("mesh");
            rigidBodyType.multiBlendTargetChild = rigidBodyType.Type.GetChildInfo("multiBlendTarget");
            rigidBodyType.nodeChild = rigidBodyType.Type.GetChildInfo("node");
            rigidBodyType.nurbsCurveChild = rigidBodyType.Type.GetChildInfo("nurbsCurve");
            rigidBodyType.nurbsChild = rigidBodyType.Type.GetChildInfo("nurbs");
            rigidBodyType.orientationConstraintChild = rigidBodyType.Type.GetChildInfo("orientationConstraint");
            rigidBodyType.parentConstraintChild = rigidBodyType.Type.GetChildInfo("parentConstraint");
            rigidBodyType.primitiveChild = rigidBodyType.Type.GetChildInfo("primitive");
            rigidBodyType.referenceChild = rigidBodyType.Type.GetChildInfo("reference");
            rigidBodyType.rigidBodyChild = rigidBodyType.Type.GetChildInfo("rigidBody");
            rigidBodyType.scaleConstraintChild = rigidBodyType.Type.GetChildInfo("scaleConstraint");
            rigidBodyType.springConstraintChild = rigidBodyType.Type.GetChildInfo("springConstraint");
            rigidBodyType.translationConstraintChild = rigidBodyType.Type.GetChildInfo("translationConstraint");
            rigidBodyType.animclipChild = rigidBodyType.Type.GetChildInfo("animclip");
            rigidBodyType.blendChild = rigidBodyType.Type.GetChildInfo("blend");
            rigidBodyType.blendshapeControllerChild = rigidBodyType.Type.GetChildInfo("blendshapeController");
            rigidBodyType.cgshaderChild = rigidBodyType.Type.GetChildInfo("cgshader");
            rigidBodyType.deformerChild = rigidBodyType.Type.GetChildInfo("deformer");
            rigidBodyType.expressionChild = rigidBodyType.Type.GetChildInfo("expression");
            rigidBodyType.imageChild = rigidBodyType.Type.GetChildInfo("image");
            rigidBodyType.materialChild = rigidBodyType.Type.GetChildInfo("material");
            rigidBodyType.motionPathChild = rigidBodyType.Type.GetChildInfo("motionPath");
            rigidBodyType.objsetChild = rigidBodyType.Type.GetChildInfo("objset");
            rigidBodyType.pmdataATGChild = rigidBodyType.Type.GetChildInfo("pmdataATG");
            rigidBodyType.poseChild = rigidBodyType.Type.GetChildInfo("pose");
            rigidBodyType.sceneChild = rigidBodyType.Type.GetChildInfo("scene");
            rigidBodyType.shaderChild = rigidBodyType.Type.GetChildInfo("shader");
            rigidBodyType.textureChild = rigidBodyType.Type.GetChildInfo("texture");
            rigidBodyType.blendshapeChild = rigidBodyType.Type.GetChildInfo("blendshape");
            rigidBodyType.skinChild = rigidBodyType.Type.GetChildInfo("skin");
            rigidBodyType.customDataChild = rigidBodyType.Type.GetChildInfo("customData");

            scaleConstraintType.Type = typeCollection.GetNodeType("scaleConstraintType");
            scaleConstraintType.nameAttribute = scaleConstraintType.Type.GetAttributeInfo("name");
            scaleConstraintType.offsetAttribute = scaleConstraintType.Type.GetAttributeInfo("offset");
            scaleConstraintType.constrainAttribute = scaleConstraintType.Type.GetAttributeInfo("constrain");
            scaleConstraintType.targetChild = scaleConstraintType.Type.GetChildInfo("target");
            scaleConstraintType.animChannelChild = scaleConstraintType.Type.GetChildInfo("animChannel");
            scaleConstraintType.animDiscontinuitiesChild = scaleConstraintType.Type.GetChildInfo("animDiscontinuities");
            scaleConstraintType.animChild = scaleConstraintType.Type.GetChildInfo("anim");
            scaleConstraintType.aimConstraintChild = scaleConstraintType.Type.GetChildInfo("aimConstraint");
            scaleConstraintType.atgilocatorChild = scaleConstraintType.Type.GetChildInfo("atgilocator");
            scaleConstraintType.blendtargetChild = scaleConstraintType.Type.GetChildInfo("blendtarget");
            scaleConstraintType.cameraChild = scaleConstraintType.Type.GetChildInfo("camera");
            scaleConstraintType.constraintChild = scaleConstraintType.Type.GetChildInfo("constraint");
            scaleConstraintType.clusterChild = scaleConstraintType.Type.GetChildInfo("cluster");
            scaleConstraintType.dynamicTypeChild = scaleConstraintType.Type.GetChildInfo("dynamicType");
            scaleConstraintType.instanceChild = scaleConstraintType.Type.GetChildInfo("instance");
            scaleConstraintType.jointChild = scaleConstraintType.Type.GetChildInfo("joint");
            scaleConstraintType.lightChild = scaleConstraintType.Type.GetChildInfo("light");
            scaleConstraintType.locatorChild = scaleConstraintType.Type.GetChildInfo("locator");
            scaleConstraintType.lodgroupChild = scaleConstraintType.Type.GetChildInfo("lodgroup");
            scaleConstraintType.meshChild = scaleConstraintType.Type.GetChildInfo("mesh");
            scaleConstraintType.multiBlendTargetChild = scaleConstraintType.Type.GetChildInfo("multiBlendTarget");
            scaleConstraintType.nodeChild = scaleConstraintType.Type.GetChildInfo("node");
            scaleConstraintType.nurbsCurveChild = scaleConstraintType.Type.GetChildInfo("nurbsCurve");
            scaleConstraintType.nurbsChild = scaleConstraintType.Type.GetChildInfo("nurbs");
            scaleConstraintType.orientationConstraintChild = scaleConstraintType.Type.GetChildInfo("orientationConstraint");
            scaleConstraintType.parentConstraintChild = scaleConstraintType.Type.GetChildInfo("parentConstraint");
            scaleConstraintType.primitiveChild = scaleConstraintType.Type.GetChildInfo("primitive");
            scaleConstraintType.referenceChild = scaleConstraintType.Type.GetChildInfo("reference");
            scaleConstraintType.rigidBodyChild = scaleConstraintType.Type.GetChildInfo("rigidBody");
            scaleConstraintType.scaleConstraintChild = scaleConstraintType.Type.GetChildInfo("scaleConstraint");
            scaleConstraintType.springConstraintChild = scaleConstraintType.Type.GetChildInfo("springConstraint");
            scaleConstraintType.translationConstraintChild = scaleConstraintType.Type.GetChildInfo("translationConstraint");
            scaleConstraintType.animclipChild = scaleConstraintType.Type.GetChildInfo("animclip");
            scaleConstraintType.blendChild = scaleConstraintType.Type.GetChildInfo("blend");
            scaleConstraintType.blendshapeControllerChild = scaleConstraintType.Type.GetChildInfo("blendshapeController");
            scaleConstraintType.cgshaderChild = scaleConstraintType.Type.GetChildInfo("cgshader");
            scaleConstraintType.deformerChild = scaleConstraintType.Type.GetChildInfo("deformer");
            scaleConstraintType.expressionChild = scaleConstraintType.Type.GetChildInfo("expression");
            scaleConstraintType.imageChild = scaleConstraintType.Type.GetChildInfo("image");
            scaleConstraintType.materialChild = scaleConstraintType.Type.GetChildInfo("material");
            scaleConstraintType.motionPathChild = scaleConstraintType.Type.GetChildInfo("motionPath");
            scaleConstraintType.objsetChild = scaleConstraintType.Type.GetChildInfo("objset");
            scaleConstraintType.pmdataATGChild = scaleConstraintType.Type.GetChildInfo("pmdataATG");
            scaleConstraintType.poseChild = scaleConstraintType.Type.GetChildInfo("pose");
            scaleConstraintType.sceneChild = scaleConstraintType.Type.GetChildInfo("scene");
            scaleConstraintType.shaderChild = scaleConstraintType.Type.GetChildInfo("shader");
            scaleConstraintType.textureChild = scaleConstraintType.Type.GetChildInfo("texture");
            scaleConstraintType.blendshapeChild = scaleConstraintType.Type.GetChildInfo("blendshape");
            scaleConstraintType.skinChild = scaleConstraintType.Type.GetChildInfo("skin");
            scaleConstraintType.customDataChild = scaleConstraintType.Type.GetChildInfo("customData");

            springConstraintType.Type = typeCollection.GetNodeType("springConstraintType");
            springConstraintType.nameAttribute = springConstraintType.Type.GetAttributeInfo("name");
            springConstraintType.stiffnessAttribute = springConstraintType.Type.GetAttributeInfo("stiffness");
            springConstraintType.restLengthAttribute = springConstraintType.Type.GetAttributeInfo("restLength");
            springConstraintType.dampingAttribute = springConstraintType.Type.GetAttributeInfo("damping");
            springConstraintType.constrainAttribute = springConstraintType.Type.GetAttributeInfo("constrain");
            springConstraintType.targetChild = springConstraintType.Type.GetChildInfo("target");
            springConstraintType.animChannelChild = springConstraintType.Type.GetChildInfo("animChannel");
            springConstraintType.animDiscontinuitiesChild = springConstraintType.Type.GetChildInfo("animDiscontinuities");
            springConstraintType.animChild = springConstraintType.Type.GetChildInfo("anim");
            springConstraintType.aimConstraintChild = springConstraintType.Type.GetChildInfo("aimConstraint");
            springConstraintType.atgilocatorChild = springConstraintType.Type.GetChildInfo("atgilocator");
            springConstraintType.blendtargetChild = springConstraintType.Type.GetChildInfo("blendtarget");
            springConstraintType.cameraChild = springConstraintType.Type.GetChildInfo("camera");
            springConstraintType.constraintChild = springConstraintType.Type.GetChildInfo("constraint");
            springConstraintType.clusterChild = springConstraintType.Type.GetChildInfo("cluster");
            springConstraintType.dynamicTypeChild = springConstraintType.Type.GetChildInfo("dynamicType");
            springConstraintType.instanceChild = springConstraintType.Type.GetChildInfo("instance");
            springConstraintType.jointChild = springConstraintType.Type.GetChildInfo("joint");
            springConstraintType.lightChild = springConstraintType.Type.GetChildInfo("light");
            springConstraintType.locatorChild = springConstraintType.Type.GetChildInfo("locator");
            springConstraintType.lodgroupChild = springConstraintType.Type.GetChildInfo("lodgroup");
            springConstraintType.meshChild = springConstraintType.Type.GetChildInfo("mesh");
            springConstraintType.multiBlendTargetChild = springConstraintType.Type.GetChildInfo("multiBlendTarget");
            springConstraintType.nodeChild = springConstraintType.Type.GetChildInfo("node");
            springConstraintType.nurbsCurveChild = springConstraintType.Type.GetChildInfo("nurbsCurve");
            springConstraintType.nurbsChild = springConstraintType.Type.GetChildInfo("nurbs");
            springConstraintType.orientationConstraintChild = springConstraintType.Type.GetChildInfo("orientationConstraint");
            springConstraintType.parentConstraintChild = springConstraintType.Type.GetChildInfo("parentConstraint");
            springConstraintType.primitiveChild = springConstraintType.Type.GetChildInfo("primitive");
            springConstraintType.referenceChild = springConstraintType.Type.GetChildInfo("reference");
            springConstraintType.rigidBodyChild = springConstraintType.Type.GetChildInfo("rigidBody");
            springConstraintType.scaleConstraintChild = springConstraintType.Type.GetChildInfo("scaleConstraint");
            springConstraintType.springConstraintChild = springConstraintType.Type.GetChildInfo("springConstraint");
            springConstraintType.translationConstraintChild = springConstraintType.Type.GetChildInfo("translationConstraint");
            springConstraintType.animclipChild = springConstraintType.Type.GetChildInfo("animclip");
            springConstraintType.blendChild = springConstraintType.Type.GetChildInfo("blend");
            springConstraintType.blendshapeControllerChild = springConstraintType.Type.GetChildInfo("blendshapeController");
            springConstraintType.cgshaderChild = springConstraintType.Type.GetChildInfo("cgshader");
            springConstraintType.deformerChild = springConstraintType.Type.GetChildInfo("deformer");
            springConstraintType.expressionChild = springConstraintType.Type.GetChildInfo("expression");
            springConstraintType.imageChild = springConstraintType.Type.GetChildInfo("image");
            springConstraintType.materialChild = springConstraintType.Type.GetChildInfo("material");
            springConstraintType.motionPathChild = springConstraintType.Type.GetChildInfo("motionPath");
            springConstraintType.objsetChild = springConstraintType.Type.GetChildInfo("objset");
            springConstraintType.pmdataATGChild = springConstraintType.Type.GetChildInfo("pmdataATG");
            springConstraintType.poseChild = springConstraintType.Type.GetChildInfo("pose");
            springConstraintType.sceneChild = springConstraintType.Type.GetChildInfo("scene");
            springConstraintType.shaderChild = springConstraintType.Type.GetChildInfo("shader");
            springConstraintType.textureChild = springConstraintType.Type.GetChildInfo("texture");
            springConstraintType.blendshapeChild = springConstraintType.Type.GetChildInfo("blendshape");
            springConstraintType.skinChild = springConstraintType.Type.GetChildInfo("skin");
            springConstraintType.customDataChild = springConstraintType.Type.GetChildInfo("customData");

            translationConstraintType.Type = typeCollection.GetNodeType("translationConstraintType");
            translationConstraintType.nameAttribute = translationConstraintType.Type.GetAttributeInfo("name");
            translationConstraintType.offsetAttribute = translationConstraintType.Type.GetAttributeInfo("offset");
            translationConstraintType.constrainAttribute = translationConstraintType.Type.GetAttributeInfo("constrain");
            translationConstraintType.targetChild = translationConstraintType.Type.GetChildInfo("target");
            translationConstraintType.animChannelChild = translationConstraintType.Type.GetChildInfo("animChannel");
            translationConstraintType.animDiscontinuitiesChild = translationConstraintType.Type.GetChildInfo("animDiscontinuities");
            translationConstraintType.animChild = translationConstraintType.Type.GetChildInfo("anim");
            translationConstraintType.aimConstraintChild = translationConstraintType.Type.GetChildInfo("aimConstraint");
            translationConstraintType.atgilocatorChild = translationConstraintType.Type.GetChildInfo("atgilocator");
            translationConstraintType.blendtargetChild = translationConstraintType.Type.GetChildInfo("blendtarget");
            translationConstraintType.cameraChild = translationConstraintType.Type.GetChildInfo("camera");
            translationConstraintType.constraintChild = translationConstraintType.Type.GetChildInfo("constraint");
            translationConstraintType.clusterChild = translationConstraintType.Type.GetChildInfo("cluster");
            translationConstraintType.dynamicTypeChild = translationConstraintType.Type.GetChildInfo("dynamicType");
            translationConstraintType.instanceChild = translationConstraintType.Type.GetChildInfo("instance");
            translationConstraintType.jointChild = translationConstraintType.Type.GetChildInfo("joint");
            translationConstraintType.lightChild = translationConstraintType.Type.GetChildInfo("light");
            translationConstraintType.locatorChild = translationConstraintType.Type.GetChildInfo("locator");
            translationConstraintType.lodgroupChild = translationConstraintType.Type.GetChildInfo("lodgroup");
            translationConstraintType.meshChild = translationConstraintType.Type.GetChildInfo("mesh");
            translationConstraintType.multiBlendTargetChild = translationConstraintType.Type.GetChildInfo("multiBlendTarget");
            translationConstraintType.nodeChild = translationConstraintType.Type.GetChildInfo("node");
            translationConstraintType.nurbsCurveChild = translationConstraintType.Type.GetChildInfo("nurbsCurve");
            translationConstraintType.nurbsChild = translationConstraintType.Type.GetChildInfo("nurbs");
            translationConstraintType.orientationConstraintChild = translationConstraintType.Type.GetChildInfo("orientationConstraint");
            translationConstraintType.parentConstraintChild = translationConstraintType.Type.GetChildInfo("parentConstraint");
            translationConstraintType.primitiveChild = translationConstraintType.Type.GetChildInfo("primitive");
            translationConstraintType.referenceChild = translationConstraintType.Type.GetChildInfo("reference");
            translationConstraintType.rigidBodyChild = translationConstraintType.Type.GetChildInfo("rigidBody");
            translationConstraintType.scaleConstraintChild = translationConstraintType.Type.GetChildInfo("scaleConstraint");
            translationConstraintType.springConstraintChild = translationConstraintType.Type.GetChildInfo("springConstraint");
            translationConstraintType.translationConstraintChild = translationConstraintType.Type.GetChildInfo("translationConstraint");
            translationConstraintType.animclipChild = translationConstraintType.Type.GetChildInfo("animclip");
            translationConstraintType.blendChild = translationConstraintType.Type.GetChildInfo("blend");
            translationConstraintType.blendshapeControllerChild = translationConstraintType.Type.GetChildInfo("blendshapeController");
            translationConstraintType.cgshaderChild = translationConstraintType.Type.GetChildInfo("cgshader");
            translationConstraintType.deformerChild = translationConstraintType.Type.GetChildInfo("deformer");
            translationConstraintType.expressionChild = translationConstraintType.Type.GetChildInfo("expression");
            translationConstraintType.imageChild = translationConstraintType.Type.GetChildInfo("image");
            translationConstraintType.materialChild = translationConstraintType.Type.GetChildInfo("material");
            translationConstraintType.motionPathChild = translationConstraintType.Type.GetChildInfo("motionPath");
            translationConstraintType.objsetChild = translationConstraintType.Type.GetChildInfo("objset");
            translationConstraintType.pmdataATGChild = translationConstraintType.Type.GetChildInfo("pmdataATG");
            translationConstraintType.poseChild = translationConstraintType.Type.GetChildInfo("pose");
            translationConstraintType.sceneChild = translationConstraintType.Type.GetChildInfo("scene");
            translationConstraintType.shaderChild = translationConstraintType.Type.GetChildInfo("shader");
            translationConstraintType.textureChild = translationConstraintType.Type.GetChildInfo("texture");
            translationConstraintType.blendshapeChild = translationConstraintType.Type.GetChildInfo("blendshape");
            translationConstraintType.skinChild = translationConstraintType.Type.GetChildInfo("skin");
            translationConstraintType.customDataChild = translationConstraintType.Type.GetChildInfo("customData");

            animclipType.Type = typeCollection.GetNodeType("animclipType");
            animclipType.nameAttribute = animclipType.Type.GetAttributeInfo("name");
            animclipType.animChannelChild = animclipType.Type.GetChildInfo("animChannel");
            animclipType.animDiscontinuitiesChild = animclipType.Type.GetChildInfo("animDiscontinuities");
            animclipType.animChild = animclipType.Type.GetChildInfo("anim");
            animclipType.aimConstraintChild = animclipType.Type.GetChildInfo("aimConstraint");
            animclipType.atgilocatorChild = animclipType.Type.GetChildInfo("atgilocator");
            animclipType.blendtargetChild = animclipType.Type.GetChildInfo("blendtarget");
            animclipType.cameraChild = animclipType.Type.GetChildInfo("camera");
            animclipType.constraintChild = animclipType.Type.GetChildInfo("constraint");
            animclipType.clusterChild = animclipType.Type.GetChildInfo("cluster");
            animclipType.dynamicTypeChild = animclipType.Type.GetChildInfo("dynamicType");
            animclipType.instanceChild = animclipType.Type.GetChildInfo("instance");
            animclipType.jointChild = animclipType.Type.GetChildInfo("joint");
            animclipType.lightChild = animclipType.Type.GetChildInfo("light");
            animclipType.locatorChild = animclipType.Type.GetChildInfo("locator");
            animclipType.lodgroupChild = animclipType.Type.GetChildInfo("lodgroup");
            animclipType.meshChild = animclipType.Type.GetChildInfo("mesh");
            animclipType.multiBlendTargetChild = animclipType.Type.GetChildInfo("multiBlendTarget");
            animclipType.nodeChild = animclipType.Type.GetChildInfo("node");
            animclipType.nurbsCurveChild = animclipType.Type.GetChildInfo("nurbsCurve");
            animclipType.nurbsChild = animclipType.Type.GetChildInfo("nurbs");
            animclipType.orientationConstraintChild = animclipType.Type.GetChildInfo("orientationConstraint");
            animclipType.parentConstraintChild = animclipType.Type.GetChildInfo("parentConstraint");
            animclipType.primitiveChild = animclipType.Type.GetChildInfo("primitive");
            animclipType.referenceChild = animclipType.Type.GetChildInfo("reference");
            animclipType.rigidBodyChild = animclipType.Type.GetChildInfo("rigidBody");
            animclipType.scaleConstraintChild = animclipType.Type.GetChildInfo("scaleConstraint");
            animclipType.springConstraintChild = animclipType.Type.GetChildInfo("springConstraint");
            animclipType.translationConstraintChild = animclipType.Type.GetChildInfo("translationConstraint");
            animclipType.animclipChild = animclipType.Type.GetChildInfo("animclip");
            animclipType.blendChild = animclipType.Type.GetChildInfo("blend");
            animclipType.blendshapeControllerChild = animclipType.Type.GetChildInfo("blendshapeController");
            animclipType.cgshaderChild = animclipType.Type.GetChildInfo("cgshader");
            animclipType.deformerChild = animclipType.Type.GetChildInfo("deformer");
            animclipType.expressionChild = animclipType.Type.GetChildInfo("expression");
            animclipType.imageChild = animclipType.Type.GetChildInfo("image");
            animclipType.materialChild = animclipType.Type.GetChildInfo("material");
            animclipType.motionPathChild = animclipType.Type.GetChildInfo("motionPath");
            animclipType.objsetChild = animclipType.Type.GetChildInfo("objset");
            animclipType.pmdataATGChild = animclipType.Type.GetChildInfo("pmdataATG");
            animclipType.poseChild = animclipType.Type.GetChildInfo("pose");
            animclipType.sceneChild = animclipType.Type.GetChildInfo("scene");
            animclipType.shaderChild = animclipType.Type.GetChildInfo("shader");
            animclipType.textureChild = animclipType.Type.GetChildInfo("texture");
            animclipType.blendshapeChild = animclipType.Type.GetChildInfo("blendshape");
            animclipType.skinChild = animclipType.Type.GetChildInfo("skin");
            animclipType.customDataChild = animclipType.Type.GetChildInfo("customData");

            blendType.Type = typeCollection.GetNodeType("blendType");
            blendType.nameAttribute = blendType.Type.GetAttributeInfo("name");
            blendType.targetAttribute = blendType.Type.GetAttributeInfo("target");
            blendType.channelAttribute = blendType.Type.GetAttributeInfo("channel");
            blendType.inputChild = blendType.Type.GetChildInfo("input");
            blendType.animChannelChild = blendType.Type.GetChildInfo("animChannel");
            blendType.animDiscontinuitiesChild = blendType.Type.GetChildInfo("animDiscontinuities");
            blendType.animChild = blendType.Type.GetChildInfo("anim");
            blendType.aimConstraintChild = blendType.Type.GetChildInfo("aimConstraint");
            blendType.atgilocatorChild = blendType.Type.GetChildInfo("atgilocator");
            blendType.blendtargetChild = blendType.Type.GetChildInfo("blendtarget");
            blendType.cameraChild = blendType.Type.GetChildInfo("camera");
            blendType.constraintChild = blendType.Type.GetChildInfo("constraint");
            blendType.clusterChild = blendType.Type.GetChildInfo("cluster");
            blendType.dynamicTypeChild = blendType.Type.GetChildInfo("dynamicType");
            blendType.instanceChild = blendType.Type.GetChildInfo("instance");
            blendType.jointChild = blendType.Type.GetChildInfo("joint");
            blendType.lightChild = blendType.Type.GetChildInfo("light");
            blendType.locatorChild = blendType.Type.GetChildInfo("locator");
            blendType.lodgroupChild = blendType.Type.GetChildInfo("lodgroup");
            blendType.meshChild = blendType.Type.GetChildInfo("mesh");
            blendType.multiBlendTargetChild = blendType.Type.GetChildInfo("multiBlendTarget");
            blendType.nodeChild = blendType.Type.GetChildInfo("node");
            blendType.nurbsCurveChild = blendType.Type.GetChildInfo("nurbsCurve");
            blendType.nurbsChild = blendType.Type.GetChildInfo("nurbs");
            blendType.orientationConstraintChild = blendType.Type.GetChildInfo("orientationConstraint");
            blendType.parentConstraintChild = blendType.Type.GetChildInfo("parentConstraint");
            blendType.primitiveChild = blendType.Type.GetChildInfo("primitive");
            blendType.referenceChild = blendType.Type.GetChildInfo("reference");
            blendType.rigidBodyChild = blendType.Type.GetChildInfo("rigidBody");
            blendType.scaleConstraintChild = blendType.Type.GetChildInfo("scaleConstraint");
            blendType.springConstraintChild = blendType.Type.GetChildInfo("springConstraint");
            blendType.translationConstraintChild = blendType.Type.GetChildInfo("translationConstraint");
            blendType.animclipChild = blendType.Type.GetChildInfo("animclip");
            blendType.blendChild = blendType.Type.GetChildInfo("blend");
            blendType.blendshapeControllerChild = blendType.Type.GetChildInfo("blendshapeController");
            blendType.cgshaderChild = blendType.Type.GetChildInfo("cgshader");
            blendType.deformerChild = blendType.Type.GetChildInfo("deformer");
            blendType.expressionChild = blendType.Type.GetChildInfo("expression");
            blendType.imageChild = blendType.Type.GetChildInfo("image");
            blendType.materialChild = blendType.Type.GetChildInfo("material");
            blendType.motionPathChild = blendType.Type.GetChildInfo("motionPath");
            blendType.objsetChild = blendType.Type.GetChildInfo("objset");
            blendType.pmdataATGChild = blendType.Type.GetChildInfo("pmdataATG");
            blendType.poseChild = blendType.Type.GetChildInfo("pose");
            blendType.sceneChild = blendType.Type.GetChildInfo("scene");
            blendType.shaderChild = blendType.Type.GetChildInfo("shader");
            blendType.textureChild = blendType.Type.GetChildInfo("texture");
            blendType.blendshapeChild = blendType.Type.GetChildInfo("blendshape");
            blendType.skinChild = blendType.Type.GetChildInfo("skin");
            blendType.customDataChild = blendType.Type.GetChildInfo("customData");

            blendType_input.Type = typeCollection.GetNodeType("blendType_input");
            blendType_input.valueAttribute = blendType_input.Type.GetAttributeInfo("value");
            blendType_input.weightAttribute = blendType_input.Type.GetAttributeInfo("weight");

            blendshapeControllerType.Type = typeCollection.GetNodeType("blendshapeControllerType");
            blendshapeControllerType.nameAttribute = blendshapeControllerType.Type.GetAttributeInfo("name");
            blendshapeControllerType.weightChild = blendshapeControllerType.Type.GetChildInfo("weight");
            blendshapeControllerType.animChannelChild = blendshapeControllerType.Type.GetChildInfo("animChannel");
            blendshapeControllerType.animDiscontinuitiesChild = blendshapeControllerType.Type.GetChildInfo("animDiscontinuities");
            blendshapeControllerType.animChild = blendshapeControllerType.Type.GetChildInfo("anim");
            blendshapeControllerType.aimConstraintChild = blendshapeControllerType.Type.GetChildInfo("aimConstraint");
            blendshapeControllerType.atgilocatorChild = blendshapeControllerType.Type.GetChildInfo("atgilocator");
            blendshapeControllerType.blendtargetChild = blendshapeControllerType.Type.GetChildInfo("blendtarget");
            blendshapeControllerType.cameraChild = blendshapeControllerType.Type.GetChildInfo("camera");
            blendshapeControllerType.constraintChild = blendshapeControllerType.Type.GetChildInfo("constraint");
            blendshapeControllerType.clusterChild = blendshapeControllerType.Type.GetChildInfo("cluster");
            blendshapeControllerType.dynamicTypeChild = blendshapeControllerType.Type.GetChildInfo("dynamicType");
            blendshapeControllerType.instanceChild = blendshapeControllerType.Type.GetChildInfo("instance");
            blendshapeControllerType.jointChild = blendshapeControllerType.Type.GetChildInfo("joint");
            blendshapeControllerType.lightChild = blendshapeControllerType.Type.GetChildInfo("light");
            blendshapeControllerType.locatorChild = blendshapeControllerType.Type.GetChildInfo("locator");
            blendshapeControllerType.lodgroupChild = blendshapeControllerType.Type.GetChildInfo("lodgroup");
            blendshapeControllerType.meshChild = blendshapeControllerType.Type.GetChildInfo("mesh");
            blendshapeControllerType.multiBlendTargetChild = blendshapeControllerType.Type.GetChildInfo("multiBlendTarget");
            blendshapeControllerType.nodeChild = blendshapeControllerType.Type.GetChildInfo("node");
            blendshapeControllerType.nurbsCurveChild = blendshapeControllerType.Type.GetChildInfo("nurbsCurve");
            blendshapeControllerType.nurbsChild = blendshapeControllerType.Type.GetChildInfo("nurbs");
            blendshapeControllerType.orientationConstraintChild = blendshapeControllerType.Type.GetChildInfo("orientationConstraint");
            blendshapeControllerType.parentConstraintChild = blendshapeControllerType.Type.GetChildInfo("parentConstraint");
            blendshapeControllerType.primitiveChild = blendshapeControllerType.Type.GetChildInfo("primitive");
            blendshapeControllerType.referenceChild = blendshapeControllerType.Type.GetChildInfo("reference");
            blendshapeControllerType.rigidBodyChild = blendshapeControllerType.Type.GetChildInfo("rigidBody");
            blendshapeControllerType.scaleConstraintChild = blendshapeControllerType.Type.GetChildInfo("scaleConstraint");
            blendshapeControllerType.springConstraintChild = blendshapeControllerType.Type.GetChildInfo("springConstraint");
            blendshapeControllerType.translationConstraintChild = blendshapeControllerType.Type.GetChildInfo("translationConstraint");
            blendshapeControllerType.animclipChild = blendshapeControllerType.Type.GetChildInfo("animclip");
            blendshapeControllerType.blendChild = blendshapeControllerType.Type.GetChildInfo("blend");
            blendshapeControllerType.blendshapeControllerChild = blendshapeControllerType.Type.GetChildInfo("blendshapeController");
            blendshapeControllerType.cgshaderChild = blendshapeControllerType.Type.GetChildInfo("cgshader");
            blendshapeControllerType.deformerChild = blendshapeControllerType.Type.GetChildInfo("deformer");
            blendshapeControllerType.expressionChild = blendshapeControllerType.Type.GetChildInfo("expression");
            blendshapeControllerType.imageChild = blendshapeControllerType.Type.GetChildInfo("image");
            blendshapeControllerType.materialChild = blendshapeControllerType.Type.GetChildInfo("material");
            blendshapeControllerType.motionPathChild = blendshapeControllerType.Type.GetChildInfo("motionPath");
            blendshapeControllerType.objsetChild = blendshapeControllerType.Type.GetChildInfo("objset");
            blendshapeControllerType.pmdataATGChild = blendshapeControllerType.Type.GetChildInfo("pmdataATG");
            blendshapeControllerType.poseChild = blendshapeControllerType.Type.GetChildInfo("pose");
            blendshapeControllerType.sceneChild = blendshapeControllerType.Type.GetChildInfo("scene");
            blendshapeControllerType.shaderChild = blendshapeControllerType.Type.GetChildInfo("shader");
            blendshapeControllerType.textureChild = blendshapeControllerType.Type.GetChildInfo("texture");
            blendshapeControllerType.blendshapeChild = blendshapeControllerType.Type.GetChildInfo("blendshape");
            blendshapeControllerType.skinChild = blendshapeControllerType.Type.GetChildInfo("skin");
            blendshapeControllerType.customDataChild = blendshapeControllerType.Type.GetChildInfo("customData");

            blendshapeControllerType_weight.Type = typeCollection.GetNodeType("blendshapeControllerType_weight");
            blendshapeControllerType_weight.weightNameAttribute = blendshapeControllerType_weight.Type.GetAttributeInfo("weightName");

            cgshaderType.Type = typeCollection.GetNodeType("cgshaderType");
            cgshaderType.nameAttribute = cgshaderType.Type.GetAttributeInfo("name");
            cgshaderType.urlAttribute = cgshaderType.Type.GetAttributeInfo("url");
            cgshaderType.bindingChild = cgshaderType.Type.GetChildInfo("binding");
            cgshaderType.animChannelChild = cgshaderType.Type.GetChildInfo("animChannel");
            cgshaderType.animDiscontinuitiesChild = cgshaderType.Type.GetChildInfo("animDiscontinuities");
            cgshaderType.animChild = cgshaderType.Type.GetChildInfo("anim");
            cgshaderType.aimConstraintChild = cgshaderType.Type.GetChildInfo("aimConstraint");
            cgshaderType.atgilocatorChild = cgshaderType.Type.GetChildInfo("atgilocator");
            cgshaderType.blendtargetChild = cgshaderType.Type.GetChildInfo("blendtarget");
            cgshaderType.cameraChild = cgshaderType.Type.GetChildInfo("camera");
            cgshaderType.constraintChild = cgshaderType.Type.GetChildInfo("constraint");
            cgshaderType.clusterChild = cgshaderType.Type.GetChildInfo("cluster");
            cgshaderType.dynamicTypeChild = cgshaderType.Type.GetChildInfo("dynamicType");
            cgshaderType.instanceChild = cgshaderType.Type.GetChildInfo("instance");
            cgshaderType.jointChild = cgshaderType.Type.GetChildInfo("joint");
            cgshaderType.lightChild = cgshaderType.Type.GetChildInfo("light");
            cgshaderType.locatorChild = cgshaderType.Type.GetChildInfo("locator");
            cgshaderType.lodgroupChild = cgshaderType.Type.GetChildInfo("lodgroup");
            cgshaderType.meshChild = cgshaderType.Type.GetChildInfo("mesh");
            cgshaderType.multiBlendTargetChild = cgshaderType.Type.GetChildInfo("multiBlendTarget");
            cgshaderType.nodeChild = cgshaderType.Type.GetChildInfo("node");
            cgshaderType.nurbsCurveChild = cgshaderType.Type.GetChildInfo("nurbsCurve");
            cgshaderType.nurbsChild = cgshaderType.Type.GetChildInfo("nurbs");
            cgshaderType.orientationConstraintChild = cgshaderType.Type.GetChildInfo("orientationConstraint");
            cgshaderType.parentConstraintChild = cgshaderType.Type.GetChildInfo("parentConstraint");
            cgshaderType.primitiveChild = cgshaderType.Type.GetChildInfo("primitive");
            cgshaderType.referenceChild = cgshaderType.Type.GetChildInfo("reference");
            cgshaderType.rigidBodyChild = cgshaderType.Type.GetChildInfo("rigidBody");
            cgshaderType.scaleConstraintChild = cgshaderType.Type.GetChildInfo("scaleConstraint");
            cgshaderType.springConstraintChild = cgshaderType.Type.GetChildInfo("springConstraint");
            cgshaderType.translationConstraintChild = cgshaderType.Type.GetChildInfo("translationConstraint");
            cgshaderType.animclipChild = cgshaderType.Type.GetChildInfo("animclip");
            cgshaderType.blendChild = cgshaderType.Type.GetChildInfo("blend");
            cgshaderType.blendshapeControllerChild = cgshaderType.Type.GetChildInfo("blendshapeController");
            cgshaderType.cgshaderChild = cgshaderType.Type.GetChildInfo("cgshader");
            cgshaderType.deformerChild = cgshaderType.Type.GetChildInfo("deformer");
            cgshaderType.expressionChild = cgshaderType.Type.GetChildInfo("expression");
            cgshaderType.imageChild = cgshaderType.Type.GetChildInfo("image");
            cgshaderType.materialChild = cgshaderType.Type.GetChildInfo("material");
            cgshaderType.motionPathChild = cgshaderType.Type.GetChildInfo("motionPath");
            cgshaderType.objsetChild = cgshaderType.Type.GetChildInfo("objset");
            cgshaderType.pmdataATGChild = cgshaderType.Type.GetChildInfo("pmdataATG");
            cgshaderType.poseChild = cgshaderType.Type.GetChildInfo("pose");
            cgshaderType.sceneChild = cgshaderType.Type.GetChildInfo("scene");
            cgshaderType.shaderChild = cgshaderType.Type.GetChildInfo("shader");
            cgshaderType.textureChild = cgshaderType.Type.GetChildInfo("texture");
            cgshaderType.blendshapeChild = cgshaderType.Type.GetChildInfo("blendshape");
            cgshaderType.skinChild = cgshaderType.Type.GetChildInfo("skin");
            cgshaderType.customDataChild = cgshaderType.Type.GetChildInfo("customData");

            cgshaderType_binding.Type = typeCollection.GetNodeType("cgshaderType_binding");
            cgshaderType_binding.Attribute = cgshaderType_binding.Type.GetAttributeInfo("");
            cgshaderType_binding.tagAttribute = cgshaderType_binding.Type.GetAttributeInfo("tag");
            cgshaderType_binding.typeAttribute = cgshaderType_binding.Type.GetAttributeInfo("type");
            cgshaderType_binding.sourceAttribute = cgshaderType_binding.Type.GetAttributeInfo("source");
            cgshaderType_binding.datasetAttribute = cgshaderType_binding.Type.GetAttributeInfo("dataset");
            cgshaderType_binding.countAttribute = cgshaderType_binding.Type.GetAttributeInfo("count");

            deformerType.Type = typeCollection.GetNodeType("deformerType");
            deformerType.nameAttribute = deformerType.Type.GetAttributeInfo("name");
            deformerType.targetAttribute = deformerType.Type.GetAttributeInfo("target");
            deformerType.animChannelChild = deformerType.Type.GetChildInfo("animChannel");
            deformerType.animDiscontinuitiesChild = deformerType.Type.GetChildInfo("animDiscontinuities");
            deformerType.animChild = deformerType.Type.GetChildInfo("anim");
            deformerType.aimConstraintChild = deformerType.Type.GetChildInfo("aimConstraint");
            deformerType.atgilocatorChild = deformerType.Type.GetChildInfo("atgilocator");
            deformerType.blendtargetChild = deformerType.Type.GetChildInfo("blendtarget");
            deformerType.cameraChild = deformerType.Type.GetChildInfo("camera");
            deformerType.constraintChild = deformerType.Type.GetChildInfo("constraint");
            deformerType.clusterChild = deformerType.Type.GetChildInfo("cluster");
            deformerType.dynamicTypeChild = deformerType.Type.GetChildInfo("dynamicType");
            deformerType.instanceChild = deformerType.Type.GetChildInfo("instance");
            deformerType.jointChild = deformerType.Type.GetChildInfo("joint");
            deformerType.lightChild = deformerType.Type.GetChildInfo("light");
            deformerType.locatorChild = deformerType.Type.GetChildInfo("locator");
            deformerType.lodgroupChild = deformerType.Type.GetChildInfo("lodgroup");
            deformerType.meshChild = deformerType.Type.GetChildInfo("mesh");
            deformerType.multiBlendTargetChild = deformerType.Type.GetChildInfo("multiBlendTarget");
            deformerType.nodeChild = deformerType.Type.GetChildInfo("node");
            deformerType.nurbsCurveChild = deformerType.Type.GetChildInfo("nurbsCurve");
            deformerType.nurbsChild = deformerType.Type.GetChildInfo("nurbs");
            deformerType.orientationConstraintChild = deformerType.Type.GetChildInfo("orientationConstraint");
            deformerType.parentConstraintChild = deformerType.Type.GetChildInfo("parentConstraint");
            deformerType.primitiveChild = deformerType.Type.GetChildInfo("primitive");
            deformerType.referenceChild = deformerType.Type.GetChildInfo("reference");
            deformerType.rigidBodyChild = deformerType.Type.GetChildInfo("rigidBody");
            deformerType.scaleConstraintChild = deformerType.Type.GetChildInfo("scaleConstraint");
            deformerType.springConstraintChild = deformerType.Type.GetChildInfo("springConstraint");
            deformerType.translationConstraintChild = deformerType.Type.GetChildInfo("translationConstraint");
            deformerType.animclipChild = deformerType.Type.GetChildInfo("animclip");
            deformerType.blendChild = deformerType.Type.GetChildInfo("blend");
            deformerType.blendshapeControllerChild = deformerType.Type.GetChildInfo("blendshapeController");
            deformerType.cgshaderChild = deformerType.Type.GetChildInfo("cgshader");
            deformerType.deformerChild = deformerType.Type.GetChildInfo("deformer");
            deformerType.expressionChild = deformerType.Type.GetChildInfo("expression");
            deformerType.imageChild = deformerType.Type.GetChildInfo("image");
            deformerType.materialChild = deformerType.Type.GetChildInfo("material");
            deformerType.motionPathChild = deformerType.Type.GetChildInfo("motionPath");
            deformerType.objsetChild = deformerType.Type.GetChildInfo("objset");
            deformerType.pmdataATGChild = deformerType.Type.GetChildInfo("pmdataATG");
            deformerType.poseChild = deformerType.Type.GetChildInfo("pose");
            deformerType.sceneChild = deformerType.Type.GetChildInfo("scene");
            deformerType.shaderChild = deformerType.Type.GetChildInfo("shader");
            deformerType.textureChild = deformerType.Type.GetChildInfo("texture");
            deformerType.blendshapeChild = deformerType.Type.GetChildInfo("blendshape");
            deformerType.skinChild = deformerType.Type.GetChildInfo("skin");
            deformerType.customDataChild = deformerType.Type.GetChildInfo("customData");

            expressionType.Type = typeCollection.GetNodeType("expressionType");
            expressionType.nameAttribute = expressionType.Type.GetAttributeInfo("name");
            expressionType.codeAttribute = expressionType.Type.GetAttributeInfo("code");
            expressionType.inputChild = expressionType.Type.GetChildInfo("input");
            expressionType.outputChild = expressionType.Type.GetChildInfo("output");
            expressionType.animChannelChild = expressionType.Type.GetChildInfo("animChannel");
            expressionType.animDiscontinuitiesChild = expressionType.Type.GetChildInfo("animDiscontinuities");
            expressionType.animChild = expressionType.Type.GetChildInfo("anim");
            expressionType.aimConstraintChild = expressionType.Type.GetChildInfo("aimConstraint");
            expressionType.atgilocatorChild = expressionType.Type.GetChildInfo("atgilocator");
            expressionType.blendtargetChild = expressionType.Type.GetChildInfo("blendtarget");
            expressionType.cameraChild = expressionType.Type.GetChildInfo("camera");
            expressionType.constraintChild = expressionType.Type.GetChildInfo("constraint");
            expressionType.clusterChild = expressionType.Type.GetChildInfo("cluster");
            expressionType.dynamicTypeChild = expressionType.Type.GetChildInfo("dynamicType");
            expressionType.instanceChild = expressionType.Type.GetChildInfo("instance");
            expressionType.jointChild = expressionType.Type.GetChildInfo("joint");
            expressionType.lightChild = expressionType.Type.GetChildInfo("light");
            expressionType.locatorChild = expressionType.Type.GetChildInfo("locator");
            expressionType.lodgroupChild = expressionType.Type.GetChildInfo("lodgroup");
            expressionType.meshChild = expressionType.Type.GetChildInfo("mesh");
            expressionType.multiBlendTargetChild = expressionType.Type.GetChildInfo("multiBlendTarget");
            expressionType.nodeChild = expressionType.Type.GetChildInfo("node");
            expressionType.nurbsCurveChild = expressionType.Type.GetChildInfo("nurbsCurve");
            expressionType.nurbsChild = expressionType.Type.GetChildInfo("nurbs");
            expressionType.orientationConstraintChild = expressionType.Type.GetChildInfo("orientationConstraint");
            expressionType.parentConstraintChild = expressionType.Type.GetChildInfo("parentConstraint");
            expressionType.primitiveChild = expressionType.Type.GetChildInfo("primitive");
            expressionType.referenceChild = expressionType.Type.GetChildInfo("reference");
            expressionType.rigidBodyChild = expressionType.Type.GetChildInfo("rigidBody");
            expressionType.scaleConstraintChild = expressionType.Type.GetChildInfo("scaleConstraint");
            expressionType.springConstraintChild = expressionType.Type.GetChildInfo("springConstraint");
            expressionType.translationConstraintChild = expressionType.Type.GetChildInfo("translationConstraint");
            expressionType.animclipChild = expressionType.Type.GetChildInfo("animclip");
            expressionType.blendChild = expressionType.Type.GetChildInfo("blend");
            expressionType.blendshapeControllerChild = expressionType.Type.GetChildInfo("blendshapeController");
            expressionType.cgshaderChild = expressionType.Type.GetChildInfo("cgshader");
            expressionType.deformerChild = expressionType.Type.GetChildInfo("deformer");
            expressionType.expressionChild = expressionType.Type.GetChildInfo("expression");
            expressionType.imageChild = expressionType.Type.GetChildInfo("image");
            expressionType.materialChild = expressionType.Type.GetChildInfo("material");
            expressionType.motionPathChild = expressionType.Type.GetChildInfo("motionPath");
            expressionType.objsetChild = expressionType.Type.GetChildInfo("objset");
            expressionType.pmdataATGChild = expressionType.Type.GetChildInfo("pmdataATG");
            expressionType.poseChild = expressionType.Type.GetChildInfo("pose");
            expressionType.sceneChild = expressionType.Type.GetChildInfo("scene");
            expressionType.shaderChild = expressionType.Type.GetChildInfo("shader");
            expressionType.textureChild = expressionType.Type.GetChildInfo("texture");
            expressionType.blendshapeChild = expressionType.Type.GetChildInfo("blendshape");
            expressionType.skinChild = expressionType.Type.GetChildInfo("skin");
            expressionType.customDataChild = expressionType.Type.GetChildInfo("customData");

            expressionType_input.Type = typeCollection.GetNodeType("expressionType_input");
            expressionType_input.objectAttribute = expressionType_input.Type.GetAttributeInfo("object");
            expressionType_input.channelAttribute = expressionType_input.Type.GetAttributeInfo("channel");

            expressionType_output.Type = typeCollection.GetNodeType("expressionType_output");
            expressionType_output.objectAttribute = expressionType_output.Type.GetAttributeInfo("object");
            expressionType_output.channelAttribute = expressionType_output.Type.GetAttributeInfo("channel");

            imageType.Type = typeCollection.GetNodeType("imageType");
            imageType.nameAttribute = imageType.Type.GetAttributeInfo("name");
            imageType.widthAttribute = imageType.Type.GetAttributeInfo("width");
            imageType.heightAttribute = imageType.Type.GetAttributeInfo("height");
            imageType.dataChild = imageType.Type.GetChildInfo("data");
            imageType.image_channelChild = imageType.Type.GetChildInfo("image_channel");

            imageType_data.Type = typeCollection.GetNodeType("imageType_data");
            imageType_data.widthAttribute = imageType_data.Type.GetAttributeInfo("width");
            imageType_data.heightAttribute = imageType_data.Type.GetAttributeInfo("height");

            image_channelType.Type = typeCollection.GetNodeType("image_channelType");
            image_channelType.nameAttribute = image_channelType.Type.GetAttributeInfo("name");
            image_channelType.dataChild = image_channelType.Type.GetChildInfo("data");

            image_channelType_data.Type = typeCollection.GetNodeType("image_channelType_data");
            image_channelType_data.Attribute = image_channelType_data.Type.GetAttributeInfo("");
            image_channelType_data.componentCountAttribute = image_channelType_data.Type.GetAttributeInfo("componentCount");
            image_channelType_data.widthAttribute = image_channelType_data.Type.GetAttributeInfo("width");
            image_channelType_data.heightAttribute = image_channelType_data.Type.GetAttributeInfo("height");

            materialType.Type = typeCollection.GetNodeType("materialType");
            materialType.nameAttribute = materialType.Type.GetAttributeInfo("name");
            materialType.urlAttribute = materialType.Type.GetAttributeInfo("url");
            materialType.matAttribute = materialType.Type.GetAttributeInfo("mat");
            materialType.frompresetAttribute = materialType.Type.GetAttributeInfo("frompreset");
            materialType.preseturlAttribute = materialType.Type.GetAttributeInfo("preseturl");
            materialType.presetAttribute = materialType.Type.GetAttributeInfo("preset");
            materialType.renderstateChild = materialType.Type.GetChildInfo("renderstate");
            materialType.bindingChild = materialType.Type.GetChildInfo("binding");
            materialType.animChannelChild = materialType.Type.GetChildInfo("animChannel");
            materialType.animDiscontinuitiesChild = materialType.Type.GetChildInfo("animDiscontinuities");
            materialType.animChild = materialType.Type.GetChildInfo("anim");
            materialType.aimConstraintChild = materialType.Type.GetChildInfo("aimConstraint");
            materialType.atgilocatorChild = materialType.Type.GetChildInfo("atgilocator");
            materialType.blendtargetChild = materialType.Type.GetChildInfo("blendtarget");
            materialType.cameraChild = materialType.Type.GetChildInfo("camera");
            materialType.constraintChild = materialType.Type.GetChildInfo("constraint");
            materialType.clusterChild = materialType.Type.GetChildInfo("cluster");
            materialType.dynamicTypeChild = materialType.Type.GetChildInfo("dynamicType");
            materialType.instanceChild = materialType.Type.GetChildInfo("instance");
            materialType.jointChild = materialType.Type.GetChildInfo("joint");
            materialType.lightChild = materialType.Type.GetChildInfo("light");
            materialType.locatorChild = materialType.Type.GetChildInfo("locator");
            materialType.lodgroupChild = materialType.Type.GetChildInfo("lodgroup");
            materialType.meshChild = materialType.Type.GetChildInfo("mesh");
            materialType.multiBlendTargetChild = materialType.Type.GetChildInfo("multiBlendTarget");
            materialType.nodeChild = materialType.Type.GetChildInfo("node");
            materialType.nurbsCurveChild = materialType.Type.GetChildInfo("nurbsCurve");
            materialType.nurbsChild = materialType.Type.GetChildInfo("nurbs");
            materialType.orientationConstraintChild = materialType.Type.GetChildInfo("orientationConstraint");
            materialType.parentConstraintChild = materialType.Type.GetChildInfo("parentConstraint");
            materialType.primitiveChild = materialType.Type.GetChildInfo("primitive");
            materialType.referenceChild = materialType.Type.GetChildInfo("reference");
            materialType.rigidBodyChild = materialType.Type.GetChildInfo("rigidBody");
            materialType.scaleConstraintChild = materialType.Type.GetChildInfo("scaleConstraint");
            materialType.springConstraintChild = materialType.Type.GetChildInfo("springConstraint");
            materialType.translationConstraintChild = materialType.Type.GetChildInfo("translationConstraint");
            materialType.animclipChild = materialType.Type.GetChildInfo("animclip");
            materialType.blendChild = materialType.Type.GetChildInfo("blend");
            materialType.blendshapeControllerChild = materialType.Type.GetChildInfo("blendshapeController");
            materialType.cgshaderChild = materialType.Type.GetChildInfo("cgshader");
            materialType.deformerChild = materialType.Type.GetChildInfo("deformer");
            materialType.expressionChild = materialType.Type.GetChildInfo("expression");
            materialType.imageChild = materialType.Type.GetChildInfo("image");
            materialType.materialChild = materialType.Type.GetChildInfo("material");
            materialType.motionPathChild = materialType.Type.GetChildInfo("motionPath");
            materialType.objsetChild = materialType.Type.GetChildInfo("objset");
            materialType.pmdataATGChild = materialType.Type.GetChildInfo("pmdataATG");
            materialType.poseChild = materialType.Type.GetChildInfo("pose");
            materialType.sceneChild = materialType.Type.GetChildInfo("scene");
            materialType.shaderChild = materialType.Type.GetChildInfo("shader");
            materialType.textureChild = materialType.Type.GetChildInfo("texture");
            materialType.blendshapeChild = materialType.Type.GetChildInfo("blendshape");
            materialType.skinChild = materialType.Type.GetChildInfo("skin");
            materialType.customDataChild = materialType.Type.GetChildInfo("customData");

            materialType_renderstate.Type = typeCollection.GetNodeType("materialType_renderstate");
            materialType_renderstate.overridenAttribute = materialType_renderstate.Type.GetAttributeInfo("overriden");
            materialType_renderstate.alphablendChild = materialType_renderstate.Type.GetChildInfo("alphablend");
            materialType_renderstate.alphatestChild = materialType_renderstate.Type.GetChildInfo("alphatest");
            materialType_renderstate.zwriteChild = materialType_renderstate.Type.GetChildInfo("zwrite");
            materialType_renderstate.ztestChild = materialType_renderstate.Type.GetChildInfo("ztest");
            materialType_renderstate.backfacecullingChild = materialType_renderstate.Type.GetChildInfo("backfaceculling");

            renderstate_alphablend.Type = typeCollection.GetNodeType("renderstate_alphablend");
            renderstate_alphablend.enabledAttribute = renderstate_alphablend.Type.GetAttributeInfo("enabled");
            renderstate_alphablend.sourceblendAttribute = renderstate_alphablend.Type.GetAttributeInfo("sourceblend");
            renderstate_alphablend.destblendAttribute = renderstate_alphablend.Type.GetAttributeInfo("destblend");

            renderstate_alphatest.Type = typeCollection.GetNodeType("renderstate_alphatest");
            renderstate_alphatest.enabledAttribute = renderstate_alphatest.Type.GetAttributeInfo("enabled");
            renderstate_alphatest.alphafuncAttribute = renderstate_alphatest.Type.GetAttributeInfo("alphafunc");
            renderstate_alphatest.alpharefAttribute = renderstate_alphatest.Type.GetAttributeInfo("alpharef");

            renderstate_zwrite.Type = typeCollection.GetNodeType("renderstate_zwrite");
            renderstate_zwrite.enabledAttribute = renderstate_zwrite.Type.GetAttributeInfo("enabled");

            renderstate_ztest.Type = typeCollection.GetNodeType("renderstate_ztest");
            renderstate_ztest.enabledAttribute = renderstate_ztest.Type.GetAttributeInfo("enabled");
            renderstate_ztest.ztestfuncAttribute = renderstate_ztest.Type.GetAttributeInfo("ztestfunc");

            renderstate_backfaceculling.Type = typeCollection.GetNodeType("renderstate_backfaceculling");
            renderstate_backfaceculling.enabledAttribute = renderstate_backfaceculling.Type.GetAttributeInfo("enabled");

            materialType_binding.Type = typeCollection.GetNodeType("materialType_binding");
            materialType_binding.Attribute = materialType_binding.Type.GetAttributeInfo("");
            materialType_binding.tagAttribute = materialType_binding.Type.GetAttributeInfo("tag");
            materialType_binding.typeAttribute = materialType_binding.Type.GetAttributeInfo("type");
            materialType_binding.datasetAttribute = materialType_binding.Type.GetAttributeInfo("dataset");
            materialType_binding.countAttribute = materialType_binding.Type.GetAttributeInfo("count");
            materialType_binding.sourceAttribute = materialType_binding.Type.GetAttributeInfo("source");

            motionPathType.Type = typeCollection.GetNodeType("motionPathType");
            motionPathType.nameAttribute = motionPathType.Type.GetAttributeInfo("name");
            motionPathType.aimAttribute = motionPathType.Type.GetAttributeInfo("aim");
            motionPathType.upAttribute = motionPathType.Type.GetAttributeInfo("up");
            motionPathType.globalupAttribute = motionPathType.Type.GetAttributeInfo("globalup");
            motionPathType.constrainAttribute = motionPathType.Type.GetAttributeInfo("constrain");
            motionPathType.parameterisationAttribute = motionPathType.Type.GetAttributeInfo("parameterisation");
            motionPathType.constrainOrientationAttribute = motionPathType.Type.GetAttributeInfo("constrainOrientation");
            motionPathType.curvePathAttribute = motionPathType.Type.GetAttributeInfo("curvePath");
            motionPathType.offsetChild = motionPathType.Type.GetChildInfo("offset");
            motionPathType.upobjectChild = motionPathType.Type.GetChildInfo("upobject");
            motionPathType.targetChild = motionPathType.Type.GetChildInfo("target");
            motionPathType.animChannelChild = motionPathType.Type.GetChildInfo("animChannel");
            motionPathType.animDiscontinuitiesChild = motionPathType.Type.GetChildInfo("animDiscontinuities");
            motionPathType.animChild = motionPathType.Type.GetChildInfo("anim");
            motionPathType.aimConstraintChild = motionPathType.Type.GetChildInfo("aimConstraint");
            motionPathType.atgilocatorChild = motionPathType.Type.GetChildInfo("atgilocator");
            motionPathType.blendtargetChild = motionPathType.Type.GetChildInfo("blendtarget");
            motionPathType.cameraChild = motionPathType.Type.GetChildInfo("camera");
            motionPathType.constraintChild = motionPathType.Type.GetChildInfo("constraint");
            motionPathType.clusterChild = motionPathType.Type.GetChildInfo("cluster");
            motionPathType.dynamicTypeChild = motionPathType.Type.GetChildInfo("dynamicType");
            motionPathType.instanceChild = motionPathType.Type.GetChildInfo("instance");
            motionPathType.jointChild = motionPathType.Type.GetChildInfo("joint");
            motionPathType.lightChild = motionPathType.Type.GetChildInfo("light");
            motionPathType.locatorChild = motionPathType.Type.GetChildInfo("locator");
            motionPathType.lodgroupChild = motionPathType.Type.GetChildInfo("lodgroup");
            motionPathType.meshChild = motionPathType.Type.GetChildInfo("mesh");
            motionPathType.multiBlendTargetChild = motionPathType.Type.GetChildInfo("multiBlendTarget");
            motionPathType.nodeChild = motionPathType.Type.GetChildInfo("node");
            motionPathType.nurbsCurveChild = motionPathType.Type.GetChildInfo("nurbsCurve");
            motionPathType.nurbsChild = motionPathType.Type.GetChildInfo("nurbs");
            motionPathType.orientationConstraintChild = motionPathType.Type.GetChildInfo("orientationConstraint");
            motionPathType.parentConstraintChild = motionPathType.Type.GetChildInfo("parentConstraint");
            motionPathType.primitiveChild = motionPathType.Type.GetChildInfo("primitive");
            motionPathType.referenceChild = motionPathType.Type.GetChildInfo("reference");
            motionPathType.rigidBodyChild = motionPathType.Type.GetChildInfo("rigidBody");
            motionPathType.scaleConstraintChild = motionPathType.Type.GetChildInfo("scaleConstraint");
            motionPathType.springConstraintChild = motionPathType.Type.GetChildInfo("springConstraint");
            motionPathType.translationConstraintChild = motionPathType.Type.GetChildInfo("translationConstraint");
            motionPathType.animclipChild = motionPathType.Type.GetChildInfo("animclip");
            motionPathType.blendChild = motionPathType.Type.GetChildInfo("blend");
            motionPathType.blendshapeControllerChild = motionPathType.Type.GetChildInfo("blendshapeController");
            motionPathType.cgshaderChild = motionPathType.Type.GetChildInfo("cgshader");
            motionPathType.deformerChild = motionPathType.Type.GetChildInfo("deformer");
            motionPathType.expressionChild = motionPathType.Type.GetChildInfo("expression");
            motionPathType.imageChild = motionPathType.Type.GetChildInfo("image");
            motionPathType.materialChild = motionPathType.Type.GetChildInfo("material");
            motionPathType.motionPathChild = motionPathType.Type.GetChildInfo("motionPath");
            motionPathType.objsetChild = motionPathType.Type.GetChildInfo("objset");
            motionPathType.pmdataATGChild = motionPathType.Type.GetChildInfo("pmdataATG");
            motionPathType.poseChild = motionPathType.Type.GetChildInfo("pose");
            motionPathType.sceneChild = motionPathType.Type.GetChildInfo("scene");
            motionPathType.shaderChild = motionPathType.Type.GetChildInfo("shader");
            motionPathType.textureChild = motionPathType.Type.GetChildInfo("texture");
            motionPathType.blendshapeChild = motionPathType.Type.GetChildInfo("blendshape");
            motionPathType.skinChild = motionPathType.Type.GetChildInfo("skin");
            motionPathType.customDataChild = motionPathType.Type.GetChildInfo("customData");

            motionPathType_upobject.Type = typeCollection.GetNodeType("motionPathType_upobject");
            motionPathType_upobject.nameAttribute = motionPathType_upobject.Type.GetAttributeInfo("name");
            motionPathType_upobject.transformAttribute = motionPathType_upobject.Type.GetAttributeInfo("transform");

            objSetType.Type = typeCollection.GetNodeType("objSetType");
            objSetType.nameAttribute = objSetType.Type.GetAttributeInfo("name");
            objSetType.typeAttribute = objSetType.Type.GetAttributeInfo("type");
            objSetType.memberChild = objSetType.Type.GetChildInfo("member");
            objSetType.animChannelChild = objSetType.Type.GetChildInfo("animChannel");
            objSetType.animDiscontinuitiesChild = objSetType.Type.GetChildInfo("animDiscontinuities");
            objSetType.animChild = objSetType.Type.GetChildInfo("anim");
            objSetType.aimConstraintChild = objSetType.Type.GetChildInfo("aimConstraint");
            objSetType.atgilocatorChild = objSetType.Type.GetChildInfo("atgilocator");
            objSetType.blendtargetChild = objSetType.Type.GetChildInfo("blendtarget");
            objSetType.cameraChild = objSetType.Type.GetChildInfo("camera");
            objSetType.constraintChild = objSetType.Type.GetChildInfo("constraint");
            objSetType.clusterChild = objSetType.Type.GetChildInfo("cluster");
            objSetType.dynamicTypeChild = objSetType.Type.GetChildInfo("dynamicType");
            objSetType.instanceChild = objSetType.Type.GetChildInfo("instance");
            objSetType.jointChild = objSetType.Type.GetChildInfo("joint");
            objSetType.lightChild = objSetType.Type.GetChildInfo("light");
            objSetType.locatorChild = objSetType.Type.GetChildInfo("locator");
            objSetType.lodgroupChild = objSetType.Type.GetChildInfo("lodgroup");
            objSetType.meshChild = objSetType.Type.GetChildInfo("mesh");
            objSetType.multiBlendTargetChild = objSetType.Type.GetChildInfo("multiBlendTarget");
            objSetType.nodeChild = objSetType.Type.GetChildInfo("node");
            objSetType.nurbsCurveChild = objSetType.Type.GetChildInfo("nurbsCurve");
            objSetType.nurbsChild = objSetType.Type.GetChildInfo("nurbs");
            objSetType.orientationConstraintChild = objSetType.Type.GetChildInfo("orientationConstraint");
            objSetType.parentConstraintChild = objSetType.Type.GetChildInfo("parentConstraint");
            objSetType.primitiveChild = objSetType.Type.GetChildInfo("primitive");
            objSetType.referenceChild = objSetType.Type.GetChildInfo("reference");
            objSetType.rigidBodyChild = objSetType.Type.GetChildInfo("rigidBody");
            objSetType.scaleConstraintChild = objSetType.Type.GetChildInfo("scaleConstraint");
            objSetType.springConstraintChild = objSetType.Type.GetChildInfo("springConstraint");
            objSetType.translationConstraintChild = objSetType.Type.GetChildInfo("translationConstraint");
            objSetType.animclipChild = objSetType.Type.GetChildInfo("animclip");
            objSetType.blendChild = objSetType.Type.GetChildInfo("blend");
            objSetType.blendshapeControllerChild = objSetType.Type.GetChildInfo("blendshapeController");
            objSetType.cgshaderChild = objSetType.Type.GetChildInfo("cgshader");
            objSetType.deformerChild = objSetType.Type.GetChildInfo("deformer");
            objSetType.expressionChild = objSetType.Type.GetChildInfo("expression");
            objSetType.imageChild = objSetType.Type.GetChildInfo("image");
            objSetType.materialChild = objSetType.Type.GetChildInfo("material");
            objSetType.motionPathChild = objSetType.Type.GetChildInfo("motionPath");
            objSetType.objsetChild = objSetType.Type.GetChildInfo("objset");
            objSetType.pmdataATGChild = objSetType.Type.GetChildInfo("pmdataATG");
            objSetType.poseChild = objSetType.Type.GetChildInfo("pose");
            objSetType.sceneChild = objSetType.Type.GetChildInfo("scene");
            objSetType.shaderChild = objSetType.Type.GetChildInfo("shader");
            objSetType.textureChild = objSetType.Type.GetChildInfo("texture");
            objSetType.blendshapeChild = objSetType.Type.GetChildInfo("blendshape");
            objSetType.skinChild = objSetType.Type.GetChildInfo("skin");
            objSetType.customDataChild = objSetType.Type.GetChildInfo("customData");

            objSetType_member.Type = typeCollection.GetNodeType("objSetType_member");
            objSetType_member.pathAttribute = objSetType_member.Type.GetAttributeInfo("path");
            objSetType_member.membershipAttribute = objSetType_member.Type.GetAttributeInfo("membership");

            pmdataATGType.Type = typeCollection.GetNodeType("pmdataATGType");
            pmdataATGType.nameAttribute = pmdataATGType.Type.GetAttributeInfo("name");
            pmdataATGType.vsAttribute = pmdataATGType.Type.GetAttributeInfo("vs");
            pmdataATGType.vtAttribute = pmdataATGType.Type.GetAttributeInfo("vt");
            pmdataATGType.facesAttribute = pmdataATGType.Type.GetAttributeInfo("faces");
            pmdataATGType.costsAttribute = pmdataATGType.Type.GetAttributeInfo("costs");
            pmdataATGType.splitsAttribute = pmdataATGType.Type.GetAttributeInfo("splits");
            pmdataATGType.fixFacesAttribute = pmdataATGType.Type.GetAttributeInfo("fixFaces");
            pmdataATGType.targetAttribute = pmdataATGType.Type.GetAttributeInfo("target");
            pmdataATGType.animChannelChild = pmdataATGType.Type.GetChildInfo("animChannel");
            pmdataATGType.animDiscontinuitiesChild = pmdataATGType.Type.GetChildInfo("animDiscontinuities");
            pmdataATGType.animChild = pmdataATGType.Type.GetChildInfo("anim");
            pmdataATGType.aimConstraintChild = pmdataATGType.Type.GetChildInfo("aimConstraint");
            pmdataATGType.atgilocatorChild = pmdataATGType.Type.GetChildInfo("atgilocator");
            pmdataATGType.blendtargetChild = pmdataATGType.Type.GetChildInfo("blendtarget");
            pmdataATGType.cameraChild = pmdataATGType.Type.GetChildInfo("camera");
            pmdataATGType.constraintChild = pmdataATGType.Type.GetChildInfo("constraint");
            pmdataATGType.clusterChild = pmdataATGType.Type.GetChildInfo("cluster");
            pmdataATGType.dynamicTypeChild = pmdataATGType.Type.GetChildInfo("dynamicType");
            pmdataATGType.instanceChild = pmdataATGType.Type.GetChildInfo("instance");
            pmdataATGType.jointChild = pmdataATGType.Type.GetChildInfo("joint");
            pmdataATGType.lightChild = pmdataATGType.Type.GetChildInfo("light");
            pmdataATGType.locatorChild = pmdataATGType.Type.GetChildInfo("locator");
            pmdataATGType.lodgroupChild = pmdataATGType.Type.GetChildInfo("lodgroup");
            pmdataATGType.meshChild = pmdataATGType.Type.GetChildInfo("mesh");
            pmdataATGType.multiBlendTargetChild = pmdataATGType.Type.GetChildInfo("multiBlendTarget");
            pmdataATGType.nodeChild = pmdataATGType.Type.GetChildInfo("node");
            pmdataATGType.nurbsCurveChild = pmdataATGType.Type.GetChildInfo("nurbsCurve");
            pmdataATGType.nurbsChild = pmdataATGType.Type.GetChildInfo("nurbs");
            pmdataATGType.orientationConstraintChild = pmdataATGType.Type.GetChildInfo("orientationConstraint");
            pmdataATGType.parentConstraintChild = pmdataATGType.Type.GetChildInfo("parentConstraint");
            pmdataATGType.primitiveChild = pmdataATGType.Type.GetChildInfo("primitive");
            pmdataATGType.referenceChild = pmdataATGType.Type.GetChildInfo("reference");
            pmdataATGType.rigidBodyChild = pmdataATGType.Type.GetChildInfo("rigidBody");
            pmdataATGType.scaleConstraintChild = pmdataATGType.Type.GetChildInfo("scaleConstraint");
            pmdataATGType.springConstraintChild = pmdataATGType.Type.GetChildInfo("springConstraint");
            pmdataATGType.translationConstraintChild = pmdataATGType.Type.GetChildInfo("translationConstraint");
            pmdataATGType.animclipChild = pmdataATGType.Type.GetChildInfo("animclip");
            pmdataATGType.blendChild = pmdataATGType.Type.GetChildInfo("blend");
            pmdataATGType.blendshapeControllerChild = pmdataATGType.Type.GetChildInfo("blendshapeController");
            pmdataATGType.cgshaderChild = pmdataATGType.Type.GetChildInfo("cgshader");
            pmdataATGType.deformerChild = pmdataATGType.Type.GetChildInfo("deformer");
            pmdataATGType.expressionChild = pmdataATGType.Type.GetChildInfo("expression");
            pmdataATGType.imageChild = pmdataATGType.Type.GetChildInfo("image");
            pmdataATGType.materialChild = pmdataATGType.Type.GetChildInfo("material");
            pmdataATGType.motionPathChild = pmdataATGType.Type.GetChildInfo("motionPath");
            pmdataATGType.objsetChild = pmdataATGType.Type.GetChildInfo("objset");
            pmdataATGType.pmdataATGChild = pmdataATGType.Type.GetChildInfo("pmdataATG");
            pmdataATGType.poseChild = pmdataATGType.Type.GetChildInfo("pose");
            pmdataATGType.sceneChild = pmdataATGType.Type.GetChildInfo("scene");
            pmdataATGType.shaderChild = pmdataATGType.Type.GetChildInfo("shader");
            pmdataATGType.textureChild = pmdataATGType.Type.GetChildInfo("texture");
            pmdataATGType.blendshapeChild = pmdataATGType.Type.GetChildInfo("blendshape");
            pmdataATGType.skinChild = pmdataATGType.Type.GetChildInfo("skin");
            pmdataATGType.customDataChild = pmdataATGType.Type.GetChildInfo("customData");

            poseType.Type = typeCollection.GetNodeType("poseType");
            poseType.nameAttribute = poseType.Type.GetAttributeInfo("name");
            poseType.bindPoseAttribute = poseType.Type.GetAttributeInfo("bindPose");
            poseType.elementChild = poseType.Type.GetChildInfo("element");
            poseType.animChannelChild = poseType.Type.GetChildInfo("animChannel");
            poseType.animDiscontinuitiesChild = poseType.Type.GetChildInfo("animDiscontinuities");
            poseType.animChild = poseType.Type.GetChildInfo("anim");
            poseType.aimConstraintChild = poseType.Type.GetChildInfo("aimConstraint");
            poseType.atgilocatorChild = poseType.Type.GetChildInfo("atgilocator");
            poseType.blendtargetChild = poseType.Type.GetChildInfo("blendtarget");
            poseType.cameraChild = poseType.Type.GetChildInfo("camera");
            poseType.constraintChild = poseType.Type.GetChildInfo("constraint");
            poseType.clusterChild = poseType.Type.GetChildInfo("cluster");
            poseType.dynamicTypeChild = poseType.Type.GetChildInfo("dynamicType");
            poseType.instanceChild = poseType.Type.GetChildInfo("instance");
            poseType.jointChild = poseType.Type.GetChildInfo("joint");
            poseType.lightChild = poseType.Type.GetChildInfo("light");
            poseType.locatorChild = poseType.Type.GetChildInfo("locator");
            poseType.lodgroupChild = poseType.Type.GetChildInfo("lodgroup");
            poseType.meshChild = poseType.Type.GetChildInfo("mesh");
            poseType.multiBlendTargetChild = poseType.Type.GetChildInfo("multiBlendTarget");
            poseType.nodeChild = poseType.Type.GetChildInfo("node");
            poseType.nurbsCurveChild = poseType.Type.GetChildInfo("nurbsCurve");
            poseType.nurbsChild = poseType.Type.GetChildInfo("nurbs");
            poseType.orientationConstraintChild = poseType.Type.GetChildInfo("orientationConstraint");
            poseType.parentConstraintChild = poseType.Type.GetChildInfo("parentConstraint");
            poseType.primitiveChild = poseType.Type.GetChildInfo("primitive");
            poseType.referenceChild = poseType.Type.GetChildInfo("reference");
            poseType.rigidBodyChild = poseType.Type.GetChildInfo("rigidBody");
            poseType.scaleConstraintChild = poseType.Type.GetChildInfo("scaleConstraint");
            poseType.springConstraintChild = poseType.Type.GetChildInfo("springConstraint");
            poseType.translationConstraintChild = poseType.Type.GetChildInfo("translationConstraint");
            poseType.animclipChild = poseType.Type.GetChildInfo("animclip");
            poseType.blendChild = poseType.Type.GetChildInfo("blend");
            poseType.blendshapeControllerChild = poseType.Type.GetChildInfo("blendshapeController");
            poseType.cgshaderChild = poseType.Type.GetChildInfo("cgshader");
            poseType.deformerChild = poseType.Type.GetChildInfo("deformer");
            poseType.expressionChild = poseType.Type.GetChildInfo("expression");
            poseType.imageChild = poseType.Type.GetChildInfo("image");
            poseType.materialChild = poseType.Type.GetChildInfo("material");
            poseType.motionPathChild = poseType.Type.GetChildInfo("motionPath");
            poseType.objsetChild = poseType.Type.GetChildInfo("objset");
            poseType.pmdataATGChild = poseType.Type.GetChildInfo("pmdataATG");
            poseType.poseChild = poseType.Type.GetChildInfo("pose");
            poseType.sceneChild = poseType.Type.GetChildInfo("scene");
            poseType.shaderChild = poseType.Type.GetChildInfo("shader");
            poseType.textureChild = poseType.Type.GetChildInfo("texture");
            poseType.blendshapeChild = poseType.Type.GetChildInfo("blendshape");
            poseType.skinChild = poseType.Type.GetChildInfo("skin");
            poseType.customDataChild = poseType.Type.GetChildInfo("customData");

            poseType_element.Type = typeCollection.GetNodeType("poseType_element");
            poseType_element.translateAttribute = poseType_element.Type.GetAttributeInfo("translate");
            poseType_element.scaleAttribute = poseType_element.Type.GetAttributeInfo("scale");
            poseType_element.targetAttribute = poseType_element.Type.GetAttributeInfo("target");
            poseType_element.rotEulChild = poseType_element.Type.GetChildInfo("rotEul");

            element_rotEul.Type = typeCollection.GetNodeType("element_rotEul");
            element_rotEul.Attribute = element_rotEul.Type.GetAttributeInfo("");
            element_rotEul.rotOrdAttribute = element_rotEul.Type.GetAttributeInfo("rotOrd");

            sceneType.Type = typeCollection.GetNodeType("sceneType");
            sceneType.nameAttribute = sceneType.Type.GetAttributeInfo("name");
            sceneType.animChannelChild = sceneType.Type.GetChildInfo("animChannel");
            sceneType.animDiscontinuitiesChild = sceneType.Type.GetChildInfo("animDiscontinuities");
            sceneType.animChild = sceneType.Type.GetChildInfo("anim");
            sceneType.aimConstraintChild = sceneType.Type.GetChildInfo("aimConstraint");
            sceneType.atgilocatorChild = sceneType.Type.GetChildInfo("atgilocator");
            sceneType.blendtargetChild = sceneType.Type.GetChildInfo("blendtarget");
            sceneType.cameraChild = sceneType.Type.GetChildInfo("camera");
            sceneType.constraintChild = sceneType.Type.GetChildInfo("constraint");
            sceneType.clusterChild = sceneType.Type.GetChildInfo("cluster");
            sceneType.dynamicTypeChild = sceneType.Type.GetChildInfo("dynamicType");
            sceneType.instanceChild = sceneType.Type.GetChildInfo("instance");
            sceneType.jointChild = sceneType.Type.GetChildInfo("joint");
            sceneType.lightChild = sceneType.Type.GetChildInfo("light");
            sceneType.locatorChild = sceneType.Type.GetChildInfo("locator");
            sceneType.lodgroupChild = sceneType.Type.GetChildInfo("lodgroup");
            sceneType.meshChild = sceneType.Type.GetChildInfo("mesh");
            sceneType.multiBlendTargetChild = sceneType.Type.GetChildInfo("multiBlendTarget");
            sceneType.nodeChild = sceneType.Type.GetChildInfo("node");
            sceneType.nurbsCurveChild = sceneType.Type.GetChildInfo("nurbsCurve");
            sceneType.nurbsChild = sceneType.Type.GetChildInfo("nurbs");
            sceneType.orientationConstraintChild = sceneType.Type.GetChildInfo("orientationConstraint");
            sceneType.parentConstraintChild = sceneType.Type.GetChildInfo("parentConstraint");
            sceneType.primitiveChild = sceneType.Type.GetChildInfo("primitive");
            sceneType.referenceChild = sceneType.Type.GetChildInfo("reference");
            sceneType.rigidBodyChild = sceneType.Type.GetChildInfo("rigidBody");
            sceneType.scaleConstraintChild = sceneType.Type.GetChildInfo("scaleConstraint");
            sceneType.springConstraintChild = sceneType.Type.GetChildInfo("springConstraint");
            sceneType.translationConstraintChild = sceneType.Type.GetChildInfo("translationConstraint");
            sceneType.animclipChild = sceneType.Type.GetChildInfo("animclip");
            sceneType.blendChild = sceneType.Type.GetChildInfo("blend");
            sceneType.blendshapeControllerChild = sceneType.Type.GetChildInfo("blendshapeController");
            sceneType.cgshaderChild = sceneType.Type.GetChildInfo("cgshader");
            sceneType.deformerChild = sceneType.Type.GetChildInfo("deformer");
            sceneType.expressionChild = sceneType.Type.GetChildInfo("expression");
            sceneType.imageChild = sceneType.Type.GetChildInfo("image");
            sceneType.materialChild = sceneType.Type.GetChildInfo("material");
            sceneType.motionPathChild = sceneType.Type.GetChildInfo("motionPath");
            sceneType.objsetChild = sceneType.Type.GetChildInfo("objset");
            sceneType.pmdataATGChild = sceneType.Type.GetChildInfo("pmdataATG");
            sceneType.poseChild = sceneType.Type.GetChildInfo("pose");
            sceneType.sceneChild = sceneType.Type.GetChildInfo("scene");
            sceneType.shaderChild = sceneType.Type.GetChildInfo("shader");
            sceneType.textureChild = sceneType.Type.GetChildInfo("texture");
            sceneType.blendshapeChild = sceneType.Type.GetChildInfo("blendshape");
            sceneType.skinChild = sceneType.Type.GetChildInfo("skin");
            sceneType.customDataChild = sceneType.Type.GetChildInfo("customData");

            shaderType.Type = typeCollection.GetNodeType("shaderType");
            shaderType.nameAttribute = shaderType.Type.GetAttributeInfo("name");
            shaderType.bindingChild = shaderType.Type.GetChildInfo("binding");
            shaderType.animChannelChild = shaderType.Type.GetChildInfo("animChannel");
            shaderType.animDiscontinuitiesChild = shaderType.Type.GetChildInfo("animDiscontinuities");
            shaderType.animChild = shaderType.Type.GetChildInfo("anim");
            shaderType.aimConstraintChild = shaderType.Type.GetChildInfo("aimConstraint");
            shaderType.atgilocatorChild = shaderType.Type.GetChildInfo("atgilocator");
            shaderType.blendtargetChild = shaderType.Type.GetChildInfo("blendtarget");
            shaderType.cameraChild = shaderType.Type.GetChildInfo("camera");
            shaderType.constraintChild = shaderType.Type.GetChildInfo("constraint");
            shaderType.clusterChild = shaderType.Type.GetChildInfo("cluster");
            shaderType.dynamicTypeChild = shaderType.Type.GetChildInfo("dynamicType");
            shaderType.instanceChild = shaderType.Type.GetChildInfo("instance");
            shaderType.jointChild = shaderType.Type.GetChildInfo("joint");
            shaderType.lightChild = shaderType.Type.GetChildInfo("light");
            shaderType.locatorChild = shaderType.Type.GetChildInfo("locator");
            shaderType.lodgroupChild = shaderType.Type.GetChildInfo("lodgroup");
            shaderType.meshChild = shaderType.Type.GetChildInfo("mesh");
            shaderType.multiBlendTargetChild = shaderType.Type.GetChildInfo("multiBlendTarget");
            shaderType.nodeChild = shaderType.Type.GetChildInfo("node");
            shaderType.nurbsCurveChild = shaderType.Type.GetChildInfo("nurbsCurve");
            shaderType.nurbsChild = shaderType.Type.GetChildInfo("nurbs");
            shaderType.orientationConstraintChild = shaderType.Type.GetChildInfo("orientationConstraint");
            shaderType.parentConstraintChild = shaderType.Type.GetChildInfo("parentConstraint");
            shaderType.primitiveChild = shaderType.Type.GetChildInfo("primitive");
            shaderType.referenceChild = shaderType.Type.GetChildInfo("reference");
            shaderType.rigidBodyChild = shaderType.Type.GetChildInfo("rigidBody");
            shaderType.scaleConstraintChild = shaderType.Type.GetChildInfo("scaleConstraint");
            shaderType.springConstraintChild = shaderType.Type.GetChildInfo("springConstraint");
            shaderType.translationConstraintChild = shaderType.Type.GetChildInfo("translationConstraint");
            shaderType.animclipChild = shaderType.Type.GetChildInfo("animclip");
            shaderType.blendChild = shaderType.Type.GetChildInfo("blend");
            shaderType.blendshapeControllerChild = shaderType.Type.GetChildInfo("blendshapeController");
            shaderType.cgshaderChild = shaderType.Type.GetChildInfo("cgshader");
            shaderType.deformerChild = shaderType.Type.GetChildInfo("deformer");
            shaderType.expressionChild = shaderType.Type.GetChildInfo("expression");
            shaderType.imageChild = shaderType.Type.GetChildInfo("image");
            shaderType.materialChild = shaderType.Type.GetChildInfo("material");
            shaderType.motionPathChild = shaderType.Type.GetChildInfo("motionPath");
            shaderType.objsetChild = shaderType.Type.GetChildInfo("objset");
            shaderType.pmdataATGChild = shaderType.Type.GetChildInfo("pmdataATG");
            shaderType.poseChild = shaderType.Type.GetChildInfo("pose");
            shaderType.sceneChild = shaderType.Type.GetChildInfo("scene");
            shaderType.shaderChild = shaderType.Type.GetChildInfo("shader");
            shaderType.textureChild = shaderType.Type.GetChildInfo("texture");
            shaderType.blendshapeChild = shaderType.Type.GetChildInfo("blendshape");
            shaderType.skinChild = shaderType.Type.GetChildInfo("skin");
            shaderType.customDataChild = shaderType.Type.GetChildInfo("customData");

            shaderType_binding.Type = typeCollection.GetNodeType("shaderType_binding");
            shaderType_binding.Attribute = shaderType_binding.Type.GetAttributeInfo("");
            shaderType_binding.tagAttribute = shaderType_binding.Type.GetAttributeInfo("tag");
            shaderType_binding.typeAttribute = shaderType_binding.Type.GetAttributeInfo("type");
            shaderType_binding.sourceAttribute = shaderType_binding.Type.GetAttributeInfo("source");
            shaderType_binding.datasetAttribute = shaderType_binding.Type.GetAttributeInfo("dataset");
            shaderType_binding.countAttribute = shaderType_binding.Type.GetAttributeInfo("count");

            textureType.Type = typeCollection.GetNodeType("textureType");
            textureType.nameAttribute = textureType.Type.GetAttributeInfo("name");
            textureType.uriAttribute = textureType.Type.GetAttributeInfo("uri");
            textureType.animChannelChild = textureType.Type.GetChildInfo("animChannel");
            textureType.animDiscontinuitiesChild = textureType.Type.GetChildInfo("animDiscontinuities");
            textureType.animChild = textureType.Type.GetChildInfo("anim");
            textureType.aimConstraintChild = textureType.Type.GetChildInfo("aimConstraint");
            textureType.atgilocatorChild = textureType.Type.GetChildInfo("atgilocator");
            textureType.blendtargetChild = textureType.Type.GetChildInfo("blendtarget");
            textureType.cameraChild = textureType.Type.GetChildInfo("camera");
            textureType.constraintChild = textureType.Type.GetChildInfo("constraint");
            textureType.clusterChild = textureType.Type.GetChildInfo("cluster");
            textureType.dynamicTypeChild = textureType.Type.GetChildInfo("dynamicType");
            textureType.instanceChild = textureType.Type.GetChildInfo("instance");
            textureType.jointChild = textureType.Type.GetChildInfo("joint");
            textureType.lightChild = textureType.Type.GetChildInfo("light");
            textureType.locatorChild = textureType.Type.GetChildInfo("locator");
            textureType.lodgroupChild = textureType.Type.GetChildInfo("lodgroup");
            textureType.meshChild = textureType.Type.GetChildInfo("mesh");
            textureType.multiBlendTargetChild = textureType.Type.GetChildInfo("multiBlendTarget");
            textureType.nodeChild = textureType.Type.GetChildInfo("node");
            textureType.nurbsCurveChild = textureType.Type.GetChildInfo("nurbsCurve");
            textureType.nurbsChild = textureType.Type.GetChildInfo("nurbs");
            textureType.orientationConstraintChild = textureType.Type.GetChildInfo("orientationConstraint");
            textureType.parentConstraintChild = textureType.Type.GetChildInfo("parentConstraint");
            textureType.primitiveChild = textureType.Type.GetChildInfo("primitive");
            textureType.referenceChild = textureType.Type.GetChildInfo("reference");
            textureType.rigidBodyChild = textureType.Type.GetChildInfo("rigidBody");
            textureType.scaleConstraintChild = textureType.Type.GetChildInfo("scaleConstraint");
            textureType.springConstraintChild = textureType.Type.GetChildInfo("springConstraint");
            textureType.translationConstraintChild = textureType.Type.GetChildInfo("translationConstraint");
            textureType.animclipChild = textureType.Type.GetChildInfo("animclip");
            textureType.blendChild = textureType.Type.GetChildInfo("blend");
            textureType.blendshapeControllerChild = textureType.Type.GetChildInfo("blendshapeController");
            textureType.cgshaderChild = textureType.Type.GetChildInfo("cgshader");
            textureType.deformerChild = textureType.Type.GetChildInfo("deformer");
            textureType.expressionChild = textureType.Type.GetChildInfo("expression");
            textureType.imageChild = textureType.Type.GetChildInfo("image");
            textureType.materialChild = textureType.Type.GetChildInfo("material");
            textureType.motionPathChild = textureType.Type.GetChildInfo("motionPath");
            textureType.objsetChild = textureType.Type.GetChildInfo("objset");
            textureType.pmdataATGChild = textureType.Type.GetChildInfo("pmdataATG");
            textureType.poseChild = textureType.Type.GetChildInfo("pose");
            textureType.sceneChild = textureType.Type.GetChildInfo("scene");
            textureType.shaderChild = textureType.Type.GetChildInfo("shader");
            textureType.textureChild = textureType.Type.GetChildInfo("texture");
            textureType.blendshapeChild = textureType.Type.GetChildInfo("blendshape");
            textureType.skinChild = textureType.Type.GetChildInfo("skin");
            textureType.customDataChild = textureType.Type.GetChildInfo("customData");

            blendshapeType.Type = typeCollection.GetNodeType("blendshapeType");
            blendshapeType.nameAttribute = blendshapeType.Type.GetAttributeInfo("name");
            blendshapeType.controllerAttribute = blendshapeType.Type.GetAttributeInfo("controller");
            blendshapeType.targetChild = blendshapeType.Type.GetChildInfo("target");
            blendshapeType.animChannelChild = blendshapeType.Type.GetChildInfo("animChannel");
            blendshapeType.animDiscontinuitiesChild = blendshapeType.Type.GetChildInfo("animDiscontinuities");
            blendshapeType.animChild = blendshapeType.Type.GetChildInfo("anim");
            blendshapeType.aimConstraintChild = blendshapeType.Type.GetChildInfo("aimConstraint");
            blendshapeType.atgilocatorChild = blendshapeType.Type.GetChildInfo("atgilocator");
            blendshapeType.blendtargetChild = blendshapeType.Type.GetChildInfo("blendtarget");
            blendshapeType.cameraChild = blendshapeType.Type.GetChildInfo("camera");
            blendshapeType.constraintChild = blendshapeType.Type.GetChildInfo("constraint");
            blendshapeType.clusterChild = blendshapeType.Type.GetChildInfo("cluster");
            blendshapeType.dynamicTypeChild = blendshapeType.Type.GetChildInfo("dynamicType");
            blendshapeType.instanceChild = blendshapeType.Type.GetChildInfo("instance");
            blendshapeType.jointChild = blendshapeType.Type.GetChildInfo("joint");
            blendshapeType.lightChild = blendshapeType.Type.GetChildInfo("light");
            blendshapeType.locatorChild = blendshapeType.Type.GetChildInfo("locator");
            blendshapeType.lodgroupChild = blendshapeType.Type.GetChildInfo("lodgroup");
            blendshapeType.meshChild = blendshapeType.Type.GetChildInfo("mesh");
            blendshapeType.multiBlendTargetChild = blendshapeType.Type.GetChildInfo("multiBlendTarget");
            blendshapeType.nodeChild = blendshapeType.Type.GetChildInfo("node");
            blendshapeType.nurbsCurveChild = blendshapeType.Type.GetChildInfo("nurbsCurve");
            blendshapeType.nurbsChild = blendshapeType.Type.GetChildInfo("nurbs");
            blendshapeType.orientationConstraintChild = blendshapeType.Type.GetChildInfo("orientationConstraint");
            blendshapeType.parentConstraintChild = blendshapeType.Type.GetChildInfo("parentConstraint");
            blendshapeType.primitiveChild = blendshapeType.Type.GetChildInfo("primitive");
            blendshapeType.referenceChild = blendshapeType.Type.GetChildInfo("reference");
            blendshapeType.rigidBodyChild = blendshapeType.Type.GetChildInfo("rigidBody");
            blendshapeType.scaleConstraintChild = blendshapeType.Type.GetChildInfo("scaleConstraint");
            blendshapeType.springConstraintChild = blendshapeType.Type.GetChildInfo("springConstraint");
            blendshapeType.translationConstraintChild = blendshapeType.Type.GetChildInfo("translationConstraint");
            blendshapeType.animclipChild = blendshapeType.Type.GetChildInfo("animclip");
            blendshapeType.blendChild = blendshapeType.Type.GetChildInfo("blend");
            blendshapeType.blendshapeControllerChild = blendshapeType.Type.GetChildInfo("blendshapeController");
            blendshapeType.cgshaderChild = blendshapeType.Type.GetChildInfo("cgshader");
            blendshapeType.deformerChild = blendshapeType.Type.GetChildInfo("deformer");
            blendshapeType.expressionChild = blendshapeType.Type.GetChildInfo("expression");
            blendshapeType.imageChild = blendshapeType.Type.GetChildInfo("image");
            blendshapeType.materialChild = blendshapeType.Type.GetChildInfo("material");
            blendshapeType.motionPathChild = blendshapeType.Type.GetChildInfo("motionPath");
            blendshapeType.objsetChild = blendshapeType.Type.GetChildInfo("objset");
            blendshapeType.pmdataATGChild = blendshapeType.Type.GetChildInfo("pmdataATG");
            blendshapeType.poseChild = blendshapeType.Type.GetChildInfo("pose");
            blendshapeType.sceneChild = blendshapeType.Type.GetChildInfo("scene");
            blendshapeType.shaderChild = blendshapeType.Type.GetChildInfo("shader");
            blendshapeType.textureChild = blendshapeType.Type.GetChildInfo("texture");
            blendshapeType.blendshapeChild = blendshapeType.Type.GetChildInfo("blendshape");
            blendshapeType.skinChild = blendshapeType.Type.GetChildInfo("skin");
            blendshapeType.customDataChild = blendshapeType.Type.GetChildInfo("customData");

            blendshapeType_target.Type = typeCollection.GetNodeType("blendshapeType_target");
            blendshapeType_target.weightIndexAttribute = blendshapeType_target.Type.GetAttributeInfo("weightIndex");
            blendshapeType_target.animChannelChild = blendshapeType_target.Type.GetChildInfo("animChannel");
            blendshapeType_target.animDiscontinuitiesChild = blendshapeType_target.Type.GetChildInfo("animDiscontinuities");
            blendshapeType_target.animChild = blendshapeType_target.Type.GetChildInfo("anim");
            blendshapeType_target.aimConstraintChild = blendshapeType_target.Type.GetChildInfo("aimConstraint");
            blendshapeType_target.atgilocatorChild = blendshapeType_target.Type.GetChildInfo("atgilocator");
            blendshapeType_target.blendtargetChild = blendshapeType_target.Type.GetChildInfo("blendtarget");
            blendshapeType_target.cameraChild = blendshapeType_target.Type.GetChildInfo("camera");
            blendshapeType_target.constraintChild = blendshapeType_target.Type.GetChildInfo("constraint");
            blendshapeType_target.clusterChild = blendshapeType_target.Type.GetChildInfo("cluster");
            blendshapeType_target.dynamicTypeChild = blendshapeType_target.Type.GetChildInfo("dynamicType");
            blendshapeType_target.instanceChild = blendshapeType_target.Type.GetChildInfo("instance");
            blendshapeType_target.jointChild = blendshapeType_target.Type.GetChildInfo("joint");
            blendshapeType_target.lightChild = blendshapeType_target.Type.GetChildInfo("light");
            blendshapeType_target.locatorChild = blendshapeType_target.Type.GetChildInfo("locator");
            blendshapeType_target.lodgroupChild = blendshapeType_target.Type.GetChildInfo("lodgroup");
            blendshapeType_target.meshChild = blendshapeType_target.Type.GetChildInfo("mesh");
            blendshapeType_target.multiBlendTargetChild = blendshapeType_target.Type.GetChildInfo("multiBlendTarget");
            blendshapeType_target.nodeChild = blendshapeType_target.Type.GetChildInfo("node");
            blendshapeType_target.nurbsCurveChild = blendshapeType_target.Type.GetChildInfo("nurbsCurve");
            blendshapeType_target.nurbsChild = blendshapeType_target.Type.GetChildInfo("nurbs");
            blendshapeType_target.orientationConstraintChild = blendshapeType_target.Type.GetChildInfo("orientationConstraint");
            blendshapeType_target.parentConstraintChild = blendshapeType_target.Type.GetChildInfo("parentConstraint");
            blendshapeType_target.primitiveChild = blendshapeType_target.Type.GetChildInfo("primitive");
            blendshapeType_target.referenceChild = blendshapeType_target.Type.GetChildInfo("reference");
            blendshapeType_target.rigidBodyChild = blendshapeType_target.Type.GetChildInfo("rigidBody");
            blendshapeType_target.scaleConstraintChild = blendshapeType_target.Type.GetChildInfo("scaleConstraint");
            blendshapeType_target.springConstraintChild = blendshapeType_target.Type.GetChildInfo("springConstraint");
            blendshapeType_target.translationConstraintChild = blendshapeType_target.Type.GetChildInfo("translationConstraint");
            blendshapeType_target.animclipChild = blendshapeType_target.Type.GetChildInfo("animclip");
            blendshapeType_target.blendChild = blendshapeType_target.Type.GetChildInfo("blend");
            blendshapeType_target.blendshapeControllerChild = blendshapeType_target.Type.GetChildInfo("blendshapeController");
            blendshapeType_target.cgshaderChild = blendshapeType_target.Type.GetChildInfo("cgshader");
            blendshapeType_target.deformerChild = blendshapeType_target.Type.GetChildInfo("deformer");
            blendshapeType_target.expressionChild = blendshapeType_target.Type.GetChildInfo("expression");
            blendshapeType_target.imageChild = blendshapeType_target.Type.GetChildInfo("image");
            blendshapeType_target.materialChild = blendshapeType_target.Type.GetChildInfo("material");
            blendshapeType_target.motionPathChild = blendshapeType_target.Type.GetChildInfo("motionPath");
            blendshapeType_target.objsetChild = blendshapeType_target.Type.GetChildInfo("objset");
            blendshapeType_target.pmdataATGChild = blendshapeType_target.Type.GetChildInfo("pmdataATG");
            blendshapeType_target.poseChild = blendshapeType_target.Type.GetChildInfo("pose");
            blendshapeType_target.sceneChild = blendshapeType_target.Type.GetChildInfo("scene");
            blendshapeType_target.shaderChild = blendshapeType_target.Type.GetChildInfo("shader");
            blendshapeType_target.textureChild = blendshapeType_target.Type.GetChildInfo("texture");
            blendshapeType_target.blendshapeChild = blendshapeType_target.Type.GetChildInfo("blendshape");
            blendshapeType_target.skinChild = blendshapeType_target.Type.GetChildInfo("skin");
            blendshapeType_target.customDataChild = blendshapeType_target.Type.GetChildInfo("customData");

            skinType.Type = typeCollection.GetNodeType("skinType");
            skinType.nameAttribute = skinType.Type.GetAttributeInfo("name");
            skinType.typeAttribute = skinType.Type.GetAttributeInfo("type");
            skinType.influenceChild = skinType.Type.GetChildInfo("influence");
            skinType.weightsChild = skinType.Type.GetChildInfo("weights");
            skinType.componentsChild = skinType.Type.GetChildInfo("components");
            skinType.animChannelChild = skinType.Type.GetChildInfo("animChannel");
            skinType.animDiscontinuitiesChild = skinType.Type.GetChildInfo("animDiscontinuities");
            skinType.animChild = skinType.Type.GetChildInfo("anim");
            skinType.aimConstraintChild = skinType.Type.GetChildInfo("aimConstraint");
            skinType.atgilocatorChild = skinType.Type.GetChildInfo("atgilocator");
            skinType.blendtargetChild = skinType.Type.GetChildInfo("blendtarget");
            skinType.cameraChild = skinType.Type.GetChildInfo("camera");
            skinType.constraintChild = skinType.Type.GetChildInfo("constraint");
            skinType.clusterChild = skinType.Type.GetChildInfo("cluster");
            skinType.dynamicTypeChild = skinType.Type.GetChildInfo("dynamicType");
            skinType.instanceChild = skinType.Type.GetChildInfo("instance");
            skinType.jointChild = skinType.Type.GetChildInfo("joint");
            skinType.lightChild = skinType.Type.GetChildInfo("light");
            skinType.locatorChild = skinType.Type.GetChildInfo("locator");
            skinType.lodgroupChild = skinType.Type.GetChildInfo("lodgroup");
            skinType.meshChild = skinType.Type.GetChildInfo("mesh");
            skinType.multiBlendTargetChild = skinType.Type.GetChildInfo("multiBlendTarget");
            skinType.nodeChild = skinType.Type.GetChildInfo("node");
            skinType.nurbsCurveChild = skinType.Type.GetChildInfo("nurbsCurve");
            skinType.nurbsChild = skinType.Type.GetChildInfo("nurbs");
            skinType.orientationConstraintChild = skinType.Type.GetChildInfo("orientationConstraint");
            skinType.parentConstraintChild = skinType.Type.GetChildInfo("parentConstraint");
            skinType.primitiveChild = skinType.Type.GetChildInfo("primitive");
            skinType.referenceChild = skinType.Type.GetChildInfo("reference");
            skinType.rigidBodyChild = skinType.Type.GetChildInfo("rigidBody");
            skinType.scaleConstraintChild = skinType.Type.GetChildInfo("scaleConstraint");
            skinType.springConstraintChild = skinType.Type.GetChildInfo("springConstraint");
            skinType.translationConstraintChild = skinType.Type.GetChildInfo("translationConstraint");
            skinType.animclipChild = skinType.Type.GetChildInfo("animclip");
            skinType.blendChild = skinType.Type.GetChildInfo("blend");
            skinType.blendshapeControllerChild = skinType.Type.GetChildInfo("blendshapeController");
            skinType.cgshaderChild = skinType.Type.GetChildInfo("cgshader");
            skinType.deformerChild = skinType.Type.GetChildInfo("deformer");
            skinType.expressionChild = skinType.Type.GetChildInfo("expression");
            skinType.imageChild = skinType.Type.GetChildInfo("image");
            skinType.materialChild = skinType.Type.GetChildInfo("material");
            skinType.motionPathChild = skinType.Type.GetChildInfo("motionPath");
            skinType.objsetChild = skinType.Type.GetChildInfo("objset");
            skinType.pmdataATGChild = skinType.Type.GetChildInfo("pmdataATG");
            skinType.poseChild = skinType.Type.GetChildInfo("pose");
            skinType.sceneChild = skinType.Type.GetChildInfo("scene");
            skinType.shaderChild = skinType.Type.GetChildInfo("shader");
            skinType.textureChild = skinType.Type.GetChildInfo("texture");
            skinType.blendshapeChild = skinType.Type.GetChildInfo("blendshape");
            skinType.skinChild = skinType.Type.GetChildInfo("skin");
            skinType.customDataChild = skinType.Type.GetChildInfo("customData");

            skinType_influence.Type = typeCollection.GetNodeType("skinType_influence");
            skinType_influence.bindInverseAttribute = skinType_influence.Type.GetAttributeInfo("bindInverse");
            skinType_influence.targetAttribute = skinType_influence.Type.GetAttributeInfo("target");

            skinType_weights.Type = typeCollection.GetNodeType("skinType_weights");
            skinType_weights.Attribute = skinType_weights.Type.GetAttributeInfo("");
            skinType_weights.countAttribute = skinType_weights.Type.GetAttributeInfo("count");

            skinType_components.Type = typeCollection.GetNodeType("skinType_components");
            skinType_components.Attribute = skinType_components.Type.GetAttributeInfo("");
            skinType_components.countAttribute = skinType_components.Type.GetAttributeInfo("count");

            customDataType.Type = typeCollection.GetNodeType("customDataType");
            customDataType.attributeChild = customDataType.Type.GetChildInfo("attribute");

            customDataAttributeType.Type = typeCollection.GetNodeType("customDataAttributeType");
            customDataAttributeType.fieldAttribute = customDataAttributeType.Type.GetAttributeInfo("field");
            customDataAttributeType.Attribute = customDataAttributeType.Type.GetAttributeInfo("");
            customDataAttributeType.nameAttribute = customDataAttributeType.Type.GetAttributeInfo("name");
            customDataAttributeType.typeAttribute = customDataAttributeType.Type.GetAttributeInfo("type");
            customDataAttributeType.valueAttribute = customDataAttributeType.Type.GetAttributeInfo("value");
            customDataAttributeType.defaultAttribute = customDataAttributeType.Type.GetAttributeInfo("default");
            customDataAttributeType.minAttribute = customDataAttributeType.Type.GetAttributeInfo("min");
            customDataAttributeType.maxAttribute = customDataAttributeType.Type.GetAttributeInfo("max");
            customDataAttributeType.countAttribute = customDataAttributeType.Type.GetAttributeInfo("count");
            customDataAttributeType.indexAttribute = customDataAttributeType.Type.GetAttributeInfo("index");
            customDataAttributeType.isArrayAttribute = customDataAttributeType.Type.GetAttributeInfo("isArray");
            customDataAttributeType.valueChild = customDataAttributeType.Type.GetChildInfo("value");

            jointType_freedoms.Type = typeCollection.GetNodeType("jointType_freedoms");
            jointType_freedoms.channelsAttribute = jointType_freedoms.Type.GetAttributeInfo("channels");

            jointType_minrotation.Type = typeCollection.GetNodeType("jointType_minrotation");
            jointType_minrotation.Attribute = jointType_minrotation.Type.GetAttributeInfo("");
            jointType_minrotation.channelsAttribute = jointType_minrotation.Type.GetAttributeInfo("channels");

            jointType_maxrotation.Type = typeCollection.GetNodeType("jointType_maxrotation");
            jointType_maxrotation.Attribute = jointType_maxrotation.Type.GetAttributeInfo("");
            jointType_maxrotation.channelsAttribute = jointType_maxrotation.Type.GetAttributeInfo("channels");

            jointType_jointOrientEul.Type = typeCollection.GetNodeType("jointType_jointOrientEul");
            jointType_jointOrientEul.Attribute = jointType_jointOrientEul.Type.GetAttributeInfo("");
            jointType_jointOrientEul.rotOrdAttribute = jointType_jointOrientEul.Type.GetAttributeInfo("rotOrd");

            ATGRootElement = typeCollection.GetRootElement("ATG");
        }

        public static class worldType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo filenameformAttribute;
            public static AttributeInfo upaxisAttribute;
            public static ChildInfo parseaccelerationChild;
            public static ChildInfo animChannelChild;
            public static ChildInfo animDiscontinuitiesChild;
            public static ChildInfo animChild;
            public static ChildInfo aimConstraintChild;
            public static ChildInfo atgilocatorChild;
            public static ChildInfo blendtargetChild;
            public static ChildInfo cameraChild;
            public static ChildInfo constraintChild;
            public static ChildInfo clusterChild;
            public static ChildInfo dynamicTypeChild;
            public static ChildInfo instanceChild;
            public static ChildInfo jointChild;
            public static ChildInfo lightChild;
            public static ChildInfo locatorChild;
            public static ChildInfo lodgroupChild;
            public static ChildInfo meshChild;
            public static ChildInfo multiBlendTargetChild;
            public static ChildInfo nodeChild;
            public static ChildInfo nurbsCurveChild;
            public static ChildInfo nurbsChild;
            public static ChildInfo orientationConstraintChild;
            public static ChildInfo parentConstraintChild;
            public static ChildInfo primitiveChild;
            public static ChildInfo referenceChild;
            public static ChildInfo rigidBodyChild;
            public static ChildInfo scaleConstraintChild;
            public static ChildInfo springConstraintChild;
            public static ChildInfo translationConstraintChild;
            public static ChildInfo animclipChild;
            public static ChildInfo blendChild;
            public static ChildInfo blendshapeControllerChild;
            public static ChildInfo cgshaderChild;
            public static ChildInfo deformerChild;
            public static ChildInfo expressionChild;
            public static ChildInfo imageChild;
            public static ChildInfo materialChild;
            public static ChildInfo motionPathChild;
            public static ChildInfo objsetChild;
            public static ChildInfo pmdataATGChild;
            public static ChildInfo poseChild;
            public static ChildInfo sceneChild;
            public static ChildInfo shaderChild;
            public static ChildInfo textureChild;
            public static ChildInfo blendshapeChild;
            public static ChildInfo skinChild;
            public static ChildInfo customDataChild;
        }

        public static class objType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
        }

        public static class parseaccelerationType
        {
            public static DomNodeType Type;
            public static ChildInfo filereferencesChild;
        }

        public static class filereferencesType
        {
            public static DomNodeType Type;
            public static ChildInfo fileChild;
        }

        public static class fileType
        {
            public static DomNodeType Type;
            public static AttributeInfo uriAttribute;
        }

        public static class animChannelType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo channelAttribute;
            public static AttributeInfo inputObjectAttribute;
            public static AttributeInfo inputChannelAttribute;
            public static ChildInfo animDataChild;
        }

        public static class animChannelType_animData
        {
            public static DomNodeType Type;
            public static AttributeInfo keyValuesAttribute;
            public static AttributeInfo keyTimesAttribute;
            public static AttributeInfo tangentsAttribute;
            public static AttributeInfo interpAttribute;
            public static AttributeInfo numKeysAttribute;
            public static AttributeInfo keyStrideAttribute;
            public static AttributeInfo timeOffsetAttribute;
            public static AttributeInfo durationAttribute;
        }

        public static class animDiscontinuitiesType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo keyStrideAttribute;
            public static ChildInfo cornerChild;
            public static ChildInfo stepChild;
        }

        public static class animDiscontinuitiesType_corner
        {
            public static DomNodeType Type;
            public static AttributeInfo timeAttribute;
        }

        public static class animDiscontinuitiesType_step
        {
            public static DomNodeType Type;
            public static AttributeInfo beforeAttribute;
            public static AttributeInfo afterAttribute;
            public static AttributeInfo timeAttribute;
        }

        public static class animType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo targetAttribute;
            public static ChildInfo animChannelChild;
            public static ChildInfo animDiscontinuitiesChild;
            public static ChildInfo animChild;
            public static ChildInfo aimConstraintChild;
            public static ChildInfo atgilocatorChild;
            public static ChildInfo blendtargetChild;
            public static ChildInfo cameraChild;
            public static ChildInfo constraintChild;
            public static ChildInfo clusterChild;
            public static ChildInfo dynamicTypeChild;
            public static ChildInfo instanceChild;
            public static ChildInfo jointChild;
            public static ChildInfo lightChild;
            public static ChildInfo locatorChild;
            public static ChildInfo lodgroupChild;
            public static ChildInfo meshChild;
            public static ChildInfo multiBlendTargetChild;
            public static ChildInfo nodeChild;
            public static ChildInfo nurbsCurveChild;
            public static ChildInfo nurbsChild;
            public static ChildInfo orientationConstraintChild;
            public static ChildInfo parentConstraintChild;
            public static ChildInfo primitiveChild;
            public static ChildInfo referenceChild;
            public static ChildInfo rigidBodyChild;
            public static ChildInfo scaleConstraintChild;
            public static ChildInfo springConstraintChild;
            public static ChildInfo translationConstraintChild;
            public static ChildInfo animclipChild;
            public static ChildInfo blendChild;
            public static ChildInfo blendshapeControllerChild;
            public static ChildInfo cgshaderChild;
            public static ChildInfo deformerChild;
            public static ChildInfo expressionChild;
            public static ChildInfo imageChild;
            public static ChildInfo materialChild;
            public static ChildInfo motionPathChild;
            public static ChildInfo objsetChild;
            public static ChildInfo pmdataATGChild;
            public static ChildInfo poseChild;
            public static ChildInfo sceneChild;
            public static ChildInfo shaderChild;
            public static ChildInfo textureChild;
            public static ChildInfo blendshapeChild;
            public static ChildInfo skinChild;
            public static ChildInfo customDataChild;
        }

        public static class aimConstraintType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo aimAttribute;
            public static AttributeInfo upAttribute;
            public static AttributeInfo globalupAttribute;
            public static AttributeInfo constrainAttribute;
            public static ChildInfo offsetChild;
            public static ChildInfo upobjectChild;
            public static ChildInfo targetChild;
            public static ChildInfo animChannelChild;
            public static ChildInfo animDiscontinuitiesChild;
            public static ChildInfo animChild;
            public static ChildInfo aimConstraintChild;
            public static ChildInfo atgilocatorChild;
            public static ChildInfo blendtargetChild;
            public static ChildInfo cameraChild;
            public static ChildInfo constraintChild;
            public static ChildInfo clusterChild;
            public static ChildInfo dynamicTypeChild;
            public static ChildInfo instanceChild;
            public static ChildInfo jointChild;
            public static ChildInfo lightChild;
            public static ChildInfo locatorChild;
            public static ChildInfo lodgroupChild;
            public static ChildInfo meshChild;
            public static ChildInfo multiBlendTargetChild;
            public static ChildInfo nodeChild;
            public static ChildInfo nurbsCurveChild;
            public static ChildInfo nurbsChild;
            public static ChildInfo orientationConstraintChild;
            public static ChildInfo parentConstraintChild;
            public static ChildInfo primitiveChild;
            public static ChildInfo referenceChild;
            public static ChildInfo rigidBodyChild;
            public static ChildInfo scaleConstraintChild;
            public static ChildInfo springConstraintChild;
            public static ChildInfo translationConstraintChild;
            public static ChildInfo animclipChild;
            public static ChildInfo blendChild;
            public static ChildInfo blendshapeControllerChild;
            public static ChildInfo cgshaderChild;
            public static ChildInfo deformerChild;
            public static ChildInfo expressionChild;
            public static ChildInfo imageChild;
            public static ChildInfo materialChild;
            public static ChildInfo motionPathChild;
            public static ChildInfo objsetChild;
            public static ChildInfo pmdataATGChild;
            public static ChildInfo poseChild;
            public static ChildInfo sceneChild;
            public static ChildInfo shaderChild;
            public static ChildInfo textureChild;
            public static ChildInfo blendshapeChild;
            public static ChildInfo skinChild;
            public static ChildInfo customDataChild;
        }

        public static class rotationType
        {
            public static DomNodeType Type;
            public static AttributeInfo Attribute;
            public static AttributeInfo rotOrdAttribute;
        }

        public static class aimConstraintType_upobject
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo transformAttribute;
        }

        public static class constraintTargetType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo weightAttribute;
        }

        public static class atgiLocatorType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo localPositionAttribute;
            public static ChildInfo fileChild;
            public static ChildInfo animChannelChild;
            public static ChildInfo animDiscontinuitiesChild;
            public static ChildInfo animChild;
            public static ChildInfo aimConstraintChild;
            public static ChildInfo atgilocatorChild;
            public static ChildInfo blendtargetChild;
            public static ChildInfo cameraChild;
            public static ChildInfo constraintChild;
            public static ChildInfo clusterChild;
            public static ChildInfo dynamicTypeChild;
            public static ChildInfo instanceChild;
            public static ChildInfo jointChild;
            public static ChildInfo lightChild;
            public static ChildInfo locatorChild;
            public static ChildInfo lodgroupChild;
            public static ChildInfo meshChild;
            public static ChildInfo multiBlendTargetChild;
            public static ChildInfo nodeChild;
            public static ChildInfo nurbsCurveChild;
            public static ChildInfo nurbsChild;
            public static ChildInfo orientationConstraintChild;
            public static ChildInfo parentConstraintChild;
            public static ChildInfo primitiveChild;
            public static ChildInfo referenceChild;
            public static ChildInfo rigidBodyChild;
            public static ChildInfo scaleConstraintChild;
            public static ChildInfo springConstraintChild;
            public static ChildInfo translationConstraintChild;
            public static ChildInfo animclipChild;
            public static ChildInfo blendChild;
            public static ChildInfo blendshapeControllerChild;
            public static ChildInfo cgshaderChild;
            public static ChildInfo deformerChild;
            public static ChildInfo expressionChild;
            public static ChildInfo imageChild;
            public static ChildInfo materialChild;
            public static ChildInfo motionPathChild;
            public static ChildInfo objsetChild;
            public static ChildInfo pmdataATGChild;
            public static ChildInfo poseChild;
            public static ChildInfo sceneChild;
            public static ChildInfo shaderChild;
            public static ChildInfo textureChild;
            public static ChildInfo blendshapeChild;
            public static ChildInfo skinChild;
            public static ChildInfo customDataChild;
        }

        public static class atgiLocatorType_file
        {
            public static DomNodeType Type;
            public static AttributeInfo uriAttribute;
        }

        public static class blendtargetType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static ChildInfo diffChild;
        }

        public static class blendtargetType_diff
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static ChildInfo indicesChild;
            public static ChildInfo deltasChild;
        }

        public static class diff_indices
        {
            public static DomNodeType Type;
            public static AttributeInfo Attribute;
            public static AttributeInfo countAttribute;
        }

        public static class diff_deltas
        {
            public static DomNodeType Type;
            public static AttributeInfo Attribute;
            public static AttributeInfo countAttribute;
        }

        public static class cameraType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo nearClipPlaneAttribute;
            public static AttributeInfo farClipPlaneAttribute;
            public static AttributeInfo focalLengthAttribute;
            public static AttributeInfo verticalFilmApertureAttribute;
            public static AttributeInfo horizontalFilmApertureAttribute;
            public static AttributeInfo orthographicAttribute;
            public static AttributeInfo orthographicWidthAttribute;
            public static ChildInfo animChannelChild;
            public static ChildInfo animDiscontinuitiesChild;
            public static ChildInfo animChild;
            public static ChildInfo aimConstraintChild;
            public static ChildInfo atgilocatorChild;
            public static ChildInfo blendtargetChild;
            public static ChildInfo cameraChild;
            public static ChildInfo constraintChild;
            public static ChildInfo clusterChild;
            public static ChildInfo dynamicTypeChild;
            public static ChildInfo instanceChild;
            public static ChildInfo jointChild;
            public static ChildInfo lightChild;
            public static ChildInfo locatorChild;
            public static ChildInfo lodgroupChild;
            public static ChildInfo meshChild;
            public static ChildInfo multiBlendTargetChild;
            public static ChildInfo nodeChild;
            public static ChildInfo nurbsCurveChild;
            public static ChildInfo nurbsChild;
            public static ChildInfo orientationConstraintChild;
            public static ChildInfo parentConstraintChild;
            public static ChildInfo primitiveChild;
            public static ChildInfo referenceChild;
            public static ChildInfo rigidBodyChild;
            public static ChildInfo scaleConstraintChild;
            public static ChildInfo springConstraintChild;
            public static ChildInfo translationConstraintChild;
            public static ChildInfo animclipChild;
            public static ChildInfo blendChild;
            public static ChildInfo blendshapeControllerChild;
            public static ChildInfo cgshaderChild;
            public static ChildInfo deformerChild;
            public static ChildInfo expressionChild;
            public static ChildInfo imageChild;
            public static ChildInfo materialChild;
            public static ChildInfo motionPathChild;
            public static ChildInfo objsetChild;
            public static ChildInfo pmdataATGChild;
            public static ChildInfo poseChild;
            public static ChildInfo sceneChild;
            public static ChildInfo shaderChild;
            public static ChildInfo textureChild;
            public static ChildInfo blendshapeChild;
            public static ChildInfo skinChild;
            public static ChildInfo customDataChild;
        }

        public static class constraintType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo constrainAttribute;
            public static ChildInfo targetChild;
            public static ChildInfo animChannelChild;
            public static ChildInfo animDiscontinuitiesChild;
            public static ChildInfo animChild;
            public static ChildInfo aimConstraintChild;
            public static ChildInfo atgilocatorChild;
            public static ChildInfo blendtargetChild;
            public static ChildInfo cameraChild;
            public static ChildInfo constraintChild;
            public static ChildInfo clusterChild;
            public static ChildInfo dynamicTypeChild;
            public static ChildInfo instanceChild;
            public static ChildInfo jointChild;
            public static ChildInfo lightChild;
            public static ChildInfo locatorChild;
            public static ChildInfo lodgroupChild;
            public static ChildInfo meshChild;
            public static ChildInfo multiBlendTargetChild;
            public static ChildInfo nodeChild;
            public static ChildInfo nurbsCurveChild;
            public static ChildInfo nurbsChild;
            public static ChildInfo orientationConstraintChild;
            public static ChildInfo parentConstraintChild;
            public static ChildInfo primitiveChild;
            public static ChildInfo referenceChild;
            public static ChildInfo rigidBodyChild;
            public static ChildInfo scaleConstraintChild;
            public static ChildInfo springConstraintChild;
            public static ChildInfo translationConstraintChild;
            public static ChildInfo animclipChild;
            public static ChildInfo blendChild;
            public static ChildInfo blendshapeControllerChild;
            public static ChildInfo cgshaderChild;
            public static ChildInfo deformerChild;
            public static ChildInfo expressionChild;
            public static ChildInfo imageChild;
            public static ChildInfo materialChild;
            public static ChildInfo motionPathChild;
            public static ChildInfo objsetChild;
            public static ChildInfo pmdataATGChild;
            public static ChildInfo poseChild;
            public static ChildInfo sceneChild;
            public static ChildInfo shaderChild;
            public static ChildInfo textureChild;
            public static ChildInfo blendshapeChild;
            public static ChildInfo skinChild;
            public static ChildInfo customDataChild;
        }

        public static class clusterType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static ChildInfo animChannelChild;
            public static ChildInfo animDiscontinuitiesChild;
            public static ChildInfo animChild;
            public static ChildInfo aimConstraintChild;
            public static ChildInfo atgilocatorChild;
            public static ChildInfo blendtargetChild;
            public static ChildInfo cameraChild;
            public static ChildInfo constraintChild;
            public static ChildInfo clusterChild;
            public static ChildInfo dynamicTypeChild;
            public static ChildInfo instanceChild;
            public static ChildInfo jointChild;
            public static ChildInfo lightChild;
            public static ChildInfo locatorChild;
            public static ChildInfo lodgroupChild;
            public static ChildInfo meshChild;
            public static ChildInfo multiBlendTargetChild;
            public static ChildInfo nodeChild;
            public static ChildInfo nurbsCurveChild;
            public static ChildInfo nurbsChild;
            public static ChildInfo orientationConstraintChild;
            public static ChildInfo parentConstraintChild;
            public static ChildInfo primitiveChild;
            public static ChildInfo referenceChild;
            public static ChildInfo rigidBodyChild;
            public static ChildInfo scaleConstraintChild;
            public static ChildInfo springConstraintChild;
            public static ChildInfo translationConstraintChild;
            public static ChildInfo animclipChild;
            public static ChildInfo blendChild;
            public static ChildInfo blendshapeControllerChild;
            public static ChildInfo cgshaderChild;
            public static ChildInfo deformerChild;
            public static ChildInfo expressionChild;
            public static ChildInfo imageChild;
            public static ChildInfo materialChild;
            public static ChildInfo motionPathChild;
            public static ChildInfo objsetChild;
            public static ChildInfo pmdataATGChild;
            public static ChildInfo poseChild;
            public static ChildInfo sceneChild;
            public static ChildInfo shaderChild;
            public static ChildInfo textureChild;
            public static ChildInfo blendshapeChild;
            public static ChildInfo skinChild;
            public static ChildInfo customDataChild;
        }

        public static class dynamicTypeType
        {
            public static DomNodeType Type;
        }

        public static class instanceType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo targetAttribute;
            public static ChildInfo animChannelChild;
            public static ChildInfo animDiscontinuitiesChild;
            public static ChildInfo animChild;
            public static ChildInfo aimConstraintChild;
            public static ChildInfo atgilocatorChild;
            public static ChildInfo blendtargetChild;
            public static ChildInfo cameraChild;
            public static ChildInfo constraintChild;
            public static ChildInfo clusterChild;
            public static ChildInfo dynamicTypeChild;
            public static ChildInfo instanceChild;
            public static ChildInfo jointChild;
            public static ChildInfo lightChild;
            public static ChildInfo locatorChild;
            public static ChildInfo lodgroupChild;
            public static ChildInfo meshChild;
            public static ChildInfo multiBlendTargetChild;
            public static ChildInfo nodeChild;
            public static ChildInfo nurbsCurveChild;
            public static ChildInfo nurbsChild;
            public static ChildInfo orientationConstraintChild;
            public static ChildInfo parentConstraintChild;
            public static ChildInfo primitiveChild;
            public static ChildInfo referenceChild;
            public static ChildInfo rigidBodyChild;
            public static ChildInfo scaleConstraintChild;
            public static ChildInfo springConstraintChild;
            public static ChildInfo translationConstraintChild;
            public static ChildInfo animclipChild;
            public static ChildInfo blendChild;
            public static ChildInfo blendshapeControllerChild;
            public static ChildInfo cgshaderChild;
            public static ChildInfo deformerChild;
            public static ChildInfo expressionChild;
            public static ChildInfo imageChild;
            public static ChildInfo materialChild;
            public static ChildInfo motionPathChild;
            public static ChildInfo objsetChild;
            public static ChildInfo pmdataATGChild;
            public static ChildInfo poseChild;
            public static ChildInfo sceneChild;
            public static ChildInfo shaderChild;
            public static ChildInfo textureChild;
            public static ChildInfo blendshapeChild;
            public static ChildInfo skinChild;
            public static ChildInfo customDataChild;
        }

        public static class jointType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo translateAttribute;
            public static AttributeInfo scaleAttribute;
            public static AttributeInfo shearAttribute;
            public static AttributeInfo scalePivotAttribute;
            public static AttributeInfo scalePivotTranslationAttribute;
            public static AttributeInfo rotatePivotAttribute;
            public static AttributeInfo rotatePivotTranslationAttribute;
            public static AttributeInfo transformAttribute;
            public static AttributeInfo boundingBoxAttribute;
            public static AttributeInfo visibilityAttribute;
            public static AttributeInfo parentEffectAttribute;
            public static AttributeInfo scaleCompensateAttribute;
            public static ChildInfo rotEulChild;
            public static ChildInfo rotAxisEulChild;
            public static ChildInfo animChannelChild;
            public static ChildInfo animDiscontinuitiesChild;
            public static ChildInfo animChild;
            public static ChildInfo aimConstraintChild;
            public static ChildInfo atgilocatorChild;
            public static ChildInfo blendtargetChild;
            public static ChildInfo cameraChild;
            public static ChildInfo constraintChild;
            public static ChildInfo clusterChild;
            public static ChildInfo dynamicTypeChild;
            public static ChildInfo instanceChild;
            public static ChildInfo jointChild;
            public static ChildInfo lightChild;
            public static ChildInfo locatorChild;
            public static ChildInfo lodgroupChild;
            public static ChildInfo meshChild;
            public static ChildInfo multiBlendTargetChild;
            public static ChildInfo nodeChild;
            public static ChildInfo nurbsCurveChild;
            public static ChildInfo nurbsChild;
            public static ChildInfo orientationConstraintChild;
            public static ChildInfo parentConstraintChild;
            public static ChildInfo primitiveChild;
            public static ChildInfo referenceChild;
            public static ChildInfo rigidBodyChild;
            public static ChildInfo scaleConstraintChild;
            public static ChildInfo springConstraintChild;
            public static ChildInfo translationConstraintChild;
            public static ChildInfo animclipChild;
            public static ChildInfo blendChild;
            public static ChildInfo blendshapeControllerChild;
            public static ChildInfo cgshaderChild;
            public static ChildInfo deformerChild;
            public static ChildInfo expressionChild;
            public static ChildInfo imageChild;
            public static ChildInfo materialChild;
            public static ChildInfo motionPathChild;
            public static ChildInfo objsetChild;
            public static ChildInfo pmdataATGChild;
            public static ChildInfo poseChild;
            public static ChildInfo sceneChild;
            public static ChildInfo shaderChild;
            public static ChildInfo textureChild;
            public static ChildInfo blendshapeChild;
            public static ChildInfo skinChild;
            public static ChildInfo customDataChild;
            public static ChildInfo freedomsChild;
            public static ChildInfo minrotationChild;
            public static ChildInfo maxrotationChild;
            public static ChildInfo jointOrientEulChild;
        }

        public static class nodeType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo translateAttribute;
            public static AttributeInfo scaleAttribute;
            public static AttributeInfo shearAttribute;
            public static AttributeInfo scalePivotAttribute;
            public static AttributeInfo scalePivotTranslationAttribute;
            public static AttributeInfo rotatePivotAttribute;
            public static AttributeInfo rotatePivotTranslationAttribute;
            public static AttributeInfo transformAttribute;
            public static AttributeInfo boundingBoxAttribute;
            public static AttributeInfo visibilityAttribute;
            public static AttributeInfo parentEffectAttribute;
            public static ChildInfo rotEulChild;
            public static ChildInfo rotAxisEulChild;
            public static ChildInfo animChannelChild;
            public static ChildInfo animDiscontinuitiesChild;
            public static ChildInfo animChild;
            public static ChildInfo aimConstraintChild;
            public static ChildInfo atgilocatorChild;
            public static ChildInfo blendtargetChild;
            public static ChildInfo cameraChild;
            public static ChildInfo constraintChild;
            public static ChildInfo clusterChild;
            public static ChildInfo dynamicTypeChild;
            public static ChildInfo instanceChild;
            public static ChildInfo jointChild;
            public static ChildInfo lightChild;
            public static ChildInfo locatorChild;
            public static ChildInfo lodgroupChild;
            public static ChildInfo meshChild;
            public static ChildInfo multiBlendTargetChild;
            public static ChildInfo nodeChild;
            public static ChildInfo nurbsCurveChild;
            public static ChildInfo nurbsChild;
            public static ChildInfo orientationConstraintChild;
            public static ChildInfo parentConstraintChild;
            public static ChildInfo primitiveChild;
            public static ChildInfo referenceChild;
            public static ChildInfo rigidBodyChild;
            public static ChildInfo scaleConstraintChild;
            public static ChildInfo springConstraintChild;
            public static ChildInfo translationConstraintChild;
            public static ChildInfo animclipChild;
            public static ChildInfo blendChild;
            public static ChildInfo blendshapeControllerChild;
            public static ChildInfo cgshaderChild;
            public static ChildInfo deformerChild;
            public static ChildInfo expressionChild;
            public static ChildInfo imageChild;
            public static ChildInfo materialChild;
            public static ChildInfo motionPathChild;
            public static ChildInfo objsetChild;
            public static ChildInfo pmdataATGChild;
            public static ChildInfo poseChild;
            public static ChildInfo sceneChild;
            public static ChildInfo shaderChild;
            public static ChildInfo textureChild;
            public static ChildInfo blendshapeChild;
            public static ChildInfo skinChild;
            public static ChildInfo customDataChild;
        }

        public static class lightType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo intensityAttribute;
            public static AttributeInfo colourAttribute;
            public static AttributeInfo colourTextureAttribute;
            public static AttributeInfo coneAngleAttribute;
            public static AttributeInfo penumbraAngleAttribute;
            public static AttributeInfo dropOffAttribute;
            public static AttributeInfo decayRateAttribute;
            public static AttributeInfo scaleAttribute;
            public static AttributeInfo typeAttribute;
            public static AttributeInfo castShadowsAttribute;
            public static AttributeInfo hasAmbientAttribute;
            public static AttributeInfo hasDiffuseAttribute;
            public static AttributeInfo hasSpecularAttribute;
            public static ChildInfo colourRampChild;
            public static ChildInfo animChannelChild;
            public static ChildInfo animDiscontinuitiesChild;
            public static ChildInfo animChild;
            public static ChildInfo aimConstraintChild;
            public static ChildInfo atgilocatorChild;
            public static ChildInfo blendtargetChild;
            public static ChildInfo cameraChild;
            public static ChildInfo constraintChild;
            public static ChildInfo clusterChild;
            public static ChildInfo dynamicTypeChild;
            public static ChildInfo instanceChild;
            public static ChildInfo jointChild;
            public static ChildInfo lightChild;
            public static ChildInfo locatorChild;
            public static ChildInfo lodgroupChild;
            public static ChildInfo meshChild;
            public static ChildInfo multiBlendTargetChild;
            public static ChildInfo nodeChild;
            public static ChildInfo nurbsCurveChild;
            public static ChildInfo nurbsChild;
            public static ChildInfo orientationConstraintChild;
            public static ChildInfo parentConstraintChild;
            public static ChildInfo primitiveChild;
            public static ChildInfo referenceChild;
            public static ChildInfo rigidBodyChild;
            public static ChildInfo scaleConstraintChild;
            public static ChildInfo springConstraintChild;
            public static ChildInfo translationConstraintChild;
            public static ChildInfo animclipChild;
            public static ChildInfo blendChild;
            public static ChildInfo blendshapeControllerChild;
            public static ChildInfo cgshaderChild;
            public static ChildInfo deformerChild;
            public static ChildInfo expressionChild;
            public static ChildInfo imageChild;
            public static ChildInfo materialChild;
            public static ChildInfo motionPathChild;
            public static ChildInfo objsetChild;
            public static ChildInfo pmdataATGChild;
            public static ChildInfo poseChild;
            public static ChildInfo sceneChild;
            public static ChildInfo shaderChild;
            public static ChildInfo textureChild;
            public static ChildInfo blendshapeChild;
            public static ChildInfo skinChild;
            public static ChildInfo customDataChild;
        }

        public static class lightType_colourRamp
        {
            public static DomNodeType Type;
            public static AttributeInfo positionsAttribute;
            public static AttributeInfo coloursAttribute;
            public static AttributeInfo interpolationTypesAttribute;
        }

        public static class locatorType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo localPositionAttribute;
            public static ChildInfo animChannelChild;
            public static ChildInfo animDiscontinuitiesChild;
            public static ChildInfo animChild;
            public static ChildInfo aimConstraintChild;
            public static ChildInfo atgilocatorChild;
            public static ChildInfo blendtargetChild;
            public static ChildInfo cameraChild;
            public static ChildInfo constraintChild;
            public static ChildInfo clusterChild;
            public static ChildInfo dynamicTypeChild;
            public static ChildInfo instanceChild;
            public static ChildInfo jointChild;
            public static ChildInfo lightChild;
            public static ChildInfo locatorChild;
            public static ChildInfo lodgroupChild;
            public static ChildInfo meshChild;
            public static ChildInfo multiBlendTargetChild;
            public static ChildInfo nodeChild;
            public static ChildInfo nurbsCurveChild;
            public static ChildInfo nurbsChild;
            public static ChildInfo orientationConstraintChild;
            public static ChildInfo parentConstraintChild;
            public static ChildInfo primitiveChild;
            public static ChildInfo referenceChild;
            public static ChildInfo rigidBodyChild;
            public static ChildInfo scaleConstraintChild;
            public static ChildInfo springConstraintChild;
            public static ChildInfo translationConstraintChild;
            public static ChildInfo animclipChild;
            public static ChildInfo blendChild;
            public static ChildInfo blendshapeControllerChild;
            public static ChildInfo cgshaderChild;
            public static ChildInfo deformerChild;
            public static ChildInfo expressionChild;
            public static ChildInfo imageChild;
            public static ChildInfo materialChild;
            public static ChildInfo motionPathChild;
            public static ChildInfo objsetChild;
            public static ChildInfo pmdataATGChild;
            public static ChildInfo poseChild;
            public static ChildInfo sceneChild;
            public static ChildInfo shaderChild;
            public static ChildInfo textureChild;
            public static ChildInfo blendshapeChild;
            public static ChildInfo skinChild;
            public static ChildInfo customDataChild;
        }

        public static class lodgroupType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo translateAttribute;
            public static AttributeInfo scaleAttribute;
            public static AttributeInfo shearAttribute;
            public static AttributeInfo scalePivotAttribute;
            public static AttributeInfo scalePivotTranslationAttribute;
            public static AttributeInfo rotatePivotAttribute;
            public static AttributeInfo rotatePivotTranslationAttribute;
            public static ChildInfo thresholdsChild;
            public static ChildInfo rotEulChild;
            public static ChildInfo rotAxisEulChild;
            public static ChildInfo animChannelChild;
            public static ChildInfo animDiscontinuitiesChild;
            public static ChildInfo animChild;
            public static ChildInfo aimConstraintChild;
            public static ChildInfo atgilocatorChild;
            public static ChildInfo blendtargetChild;
            public static ChildInfo cameraChild;
            public static ChildInfo constraintChild;
            public static ChildInfo clusterChild;
            public static ChildInfo dynamicTypeChild;
            public static ChildInfo instanceChild;
            public static ChildInfo jointChild;
            public static ChildInfo lightChild;
            public static ChildInfo locatorChild;
            public static ChildInfo lodgroupChild;
            public static ChildInfo meshChild;
            public static ChildInfo multiBlendTargetChild;
            public static ChildInfo nodeChild;
            public static ChildInfo nurbsCurveChild;
            public static ChildInfo nurbsChild;
            public static ChildInfo orientationConstraintChild;
            public static ChildInfo parentConstraintChild;
            public static ChildInfo primitiveChild;
            public static ChildInfo referenceChild;
            public static ChildInfo rigidBodyChild;
            public static ChildInfo scaleConstraintChild;
            public static ChildInfo springConstraintChild;
            public static ChildInfo translationConstraintChild;
            public static ChildInfo animclipChild;
            public static ChildInfo blendChild;
            public static ChildInfo blendshapeControllerChild;
            public static ChildInfo cgshaderChild;
            public static ChildInfo deformerChild;
            public static ChildInfo expressionChild;
            public static ChildInfo imageChild;
            public static ChildInfo materialChild;
            public static ChildInfo motionPathChild;
            public static ChildInfo objsetChild;
            public static ChildInfo pmdataATGChild;
            public static ChildInfo poseChild;
            public static ChildInfo sceneChild;
            public static ChildInfo shaderChild;
            public static ChildInfo textureChild;
            public static ChildInfo blendshapeChild;
            public static ChildInfo skinChild;
            public static ChildInfo customDataChild;
        }

        public static class lodgroupType_thresholds
        {
            public static DomNodeType Type;
            public static AttributeInfo Attribute;
            public static AttributeInfo countAttribute;
        }

        public static class meshType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo boundingBoxAttribute;
            public static ChildInfo vertexArrayChild;
            public static ChildInfo animChannelChild;
            public static ChildInfo animDiscontinuitiesChild;
            public static ChildInfo animChild;
            public static ChildInfo aimConstraintChild;
            public static ChildInfo atgilocatorChild;
            public static ChildInfo blendtargetChild;
            public static ChildInfo cameraChild;
            public static ChildInfo constraintChild;
            public static ChildInfo clusterChild;
            public static ChildInfo dynamicTypeChild;
            public static ChildInfo instanceChild;
            public static ChildInfo jointChild;
            public static ChildInfo lightChild;
            public static ChildInfo locatorChild;
            public static ChildInfo lodgroupChild;
            public static ChildInfo meshChild;
            public static ChildInfo multiBlendTargetChild;
            public static ChildInfo nodeChild;
            public static ChildInfo nurbsCurveChild;
            public static ChildInfo nurbsChild;
            public static ChildInfo orientationConstraintChild;
            public static ChildInfo parentConstraintChild;
            public static ChildInfo primitiveChild;
            public static ChildInfo referenceChild;
            public static ChildInfo rigidBodyChild;
            public static ChildInfo scaleConstraintChild;
            public static ChildInfo springConstraintChild;
            public static ChildInfo translationConstraintChild;
            public static ChildInfo animclipChild;
            public static ChildInfo blendChild;
            public static ChildInfo blendshapeControllerChild;
            public static ChildInfo cgshaderChild;
            public static ChildInfo deformerChild;
            public static ChildInfo expressionChild;
            public static ChildInfo imageChild;
            public static ChildInfo materialChild;
            public static ChildInfo motionPathChild;
            public static ChildInfo objsetChild;
            public static ChildInfo pmdataATGChild;
            public static ChildInfo poseChild;
            public static ChildInfo sceneChild;
            public static ChildInfo shaderChild;
            public static ChildInfo textureChild;
            public static ChildInfo blendshapeChild;
            public static ChildInfo skinChild;
            public static ChildInfo customDataChild;
        }

        public static class meshType_vertexArray
        {
            public static DomNodeType Type;
            public static ChildInfo primitivesChild;
            public static ChildInfo arrayChild;
            public static ChildInfo blindDataChild;
        }

        public static class vertexArray_primitives
        {
            public static DomNodeType Type;
            public static AttributeInfo sizesAttribute;
            public static AttributeInfo indicesAttribute;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo shaderAttribute;
            public static AttributeInfo typeAttribute;
            public static AttributeInfo countAttribute;
            public static ChildInfo bindingChild;
        }

        public static class primitives_binding
        {
            public static DomNodeType Type;
            public static AttributeInfo sourceAttribute;
        }

        public static class vertexArray_array
        {
            public static DomNodeType Type;
            public static AttributeInfo Attribute;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo countAttribute;
            public static AttributeInfo strideAttribute;
        }

        public static class blindDataType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo componentsAttribute;
            public static AttributeInfo componentAttribute;
            public static AttributeInfo numComponentsAttribute;
            public static ChildInfo attributeChild;
        }

        public static class blindDataType_attribute
        {
            public static DomNodeType Type;
            public static AttributeInfo Attribute;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo typeAttribute;
        }

        public static class multiBlendTargetType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static ChildInfo blendStageChild;
            public static ChildInfo animChannelChild;
            public static ChildInfo animDiscontinuitiesChild;
            public static ChildInfo animChild;
            public static ChildInfo aimConstraintChild;
            public static ChildInfo atgilocatorChild;
            public static ChildInfo blendtargetChild;
            public static ChildInfo cameraChild;
            public static ChildInfo constraintChild;
            public static ChildInfo clusterChild;
            public static ChildInfo dynamicTypeChild;
            public static ChildInfo instanceChild;
            public static ChildInfo jointChild;
            public static ChildInfo lightChild;
            public static ChildInfo locatorChild;
            public static ChildInfo lodgroupChild;
            public static ChildInfo meshChild;
            public static ChildInfo multiBlendTargetChild;
            public static ChildInfo nodeChild;
            public static ChildInfo nurbsCurveChild;
            public static ChildInfo nurbsChild;
            public static ChildInfo orientationConstraintChild;
            public static ChildInfo parentConstraintChild;
            public static ChildInfo primitiveChild;
            public static ChildInfo referenceChild;
            public static ChildInfo rigidBodyChild;
            public static ChildInfo scaleConstraintChild;
            public static ChildInfo springConstraintChild;
            public static ChildInfo translationConstraintChild;
            public static ChildInfo animclipChild;
            public static ChildInfo blendChild;
            public static ChildInfo blendshapeControllerChild;
            public static ChildInfo cgshaderChild;
            public static ChildInfo deformerChild;
            public static ChildInfo expressionChild;
            public static ChildInfo imageChild;
            public static ChildInfo materialChild;
            public static ChildInfo motionPathChild;
            public static ChildInfo objsetChild;
            public static ChildInfo pmdataATGChild;
            public static ChildInfo poseChild;
            public static ChildInfo sceneChild;
            public static ChildInfo shaderChild;
            public static ChildInfo textureChild;
            public static ChildInfo blendshapeChild;
            public static ChildInfo skinChild;
            public static ChildInfo customDataChild;
        }

        public static class multiBlendTargetType_blendStage
        {
            public static DomNodeType Type;
            public static AttributeInfo interpolantAttribute;
            public static ChildInfo animChannelChild;
            public static ChildInfo animDiscontinuitiesChild;
            public static ChildInfo animChild;
            public static ChildInfo aimConstraintChild;
            public static ChildInfo atgilocatorChild;
            public static ChildInfo blendtargetChild;
            public static ChildInfo cameraChild;
            public static ChildInfo constraintChild;
            public static ChildInfo clusterChild;
            public static ChildInfo dynamicTypeChild;
            public static ChildInfo instanceChild;
            public static ChildInfo jointChild;
            public static ChildInfo lightChild;
            public static ChildInfo locatorChild;
            public static ChildInfo lodgroupChild;
            public static ChildInfo meshChild;
            public static ChildInfo multiBlendTargetChild;
            public static ChildInfo nodeChild;
            public static ChildInfo nurbsCurveChild;
            public static ChildInfo nurbsChild;
            public static ChildInfo orientationConstraintChild;
            public static ChildInfo parentConstraintChild;
            public static ChildInfo primitiveChild;
            public static ChildInfo referenceChild;
            public static ChildInfo rigidBodyChild;
            public static ChildInfo scaleConstraintChild;
            public static ChildInfo springConstraintChild;
            public static ChildInfo translationConstraintChild;
            public static ChildInfo animclipChild;
            public static ChildInfo blendChild;
            public static ChildInfo blendshapeControllerChild;
            public static ChildInfo cgshaderChild;
            public static ChildInfo deformerChild;
            public static ChildInfo expressionChild;
            public static ChildInfo imageChild;
            public static ChildInfo materialChild;
            public static ChildInfo motionPathChild;
            public static ChildInfo objsetChild;
            public static ChildInfo pmdataATGChild;
            public static ChildInfo poseChild;
            public static ChildInfo sceneChild;
            public static ChildInfo shaderChild;
            public static ChildInfo textureChild;
            public static ChildInfo blendshapeChild;
            public static ChildInfo skinChild;
            public static ChildInfo customDataChild;
        }

        public static class nurbsCurveType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo degreeAttribute;
            public static AttributeInfo formAttribute;
            public static ChildInfo pointsChild;
            public static ChildInfo knotsChild;
            public static ChildInfo animChannelChild;
            public static ChildInfo animDiscontinuitiesChild;
            public static ChildInfo animChild;
            public static ChildInfo aimConstraintChild;
            public static ChildInfo atgilocatorChild;
            public static ChildInfo blendtargetChild;
            public static ChildInfo cameraChild;
            public static ChildInfo constraintChild;
            public static ChildInfo clusterChild;
            public static ChildInfo dynamicTypeChild;
            public static ChildInfo instanceChild;
            public static ChildInfo jointChild;
            public static ChildInfo lightChild;
            public static ChildInfo locatorChild;
            public static ChildInfo lodgroupChild;
            public static ChildInfo meshChild;
            public static ChildInfo multiBlendTargetChild;
            public static ChildInfo nodeChild;
            public static ChildInfo nurbsCurveChild;
            public static ChildInfo nurbsChild;
            public static ChildInfo orientationConstraintChild;
            public static ChildInfo parentConstraintChild;
            public static ChildInfo primitiveChild;
            public static ChildInfo referenceChild;
            public static ChildInfo rigidBodyChild;
            public static ChildInfo scaleConstraintChild;
            public static ChildInfo springConstraintChild;
            public static ChildInfo translationConstraintChild;
            public static ChildInfo animclipChild;
            public static ChildInfo blendChild;
            public static ChildInfo blendshapeControllerChild;
            public static ChildInfo cgshaderChild;
            public static ChildInfo deformerChild;
            public static ChildInfo expressionChild;
            public static ChildInfo imageChild;
            public static ChildInfo materialChild;
            public static ChildInfo motionPathChild;
            public static ChildInfo objsetChild;
            public static ChildInfo pmdataATGChild;
            public static ChildInfo poseChild;
            public static ChildInfo sceneChild;
            public static ChildInfo shaderChild;
            public static ChildInfo textureChild;
            public static ChildInfo blendshapeChild;
            public static ChildInfo skinChild;
            public static ChildInfo customDataChild;
        }

        public static class nurbsCurveType_points
        {
            public static DomNodeType Type;
            public static AttributeInfo Attribute;
            public static AttributeInfo countAttribute;
        }

        public static class nurbsCurveType_knots
        {
            public static DomNodeType Type;
            public static AttributeInfo Attribute;
            public static AttributeInfo countAttribute;
        }

        public static class nurbsSurfaceType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static ChildInfo surfaceChild;
            public static ChildInfo animChannelChild;
            public static ChildInfo animDiscontinuitiesChild;
            public static ChildInfo animChild;
            public static ChildInfo aimConstraintChild;
            public static ChildInfo atgilocatorChild;
            public static ChildInfo blendtargetChild;
            public static ChildInfo cameraChild;
            public static ChildInfo constraintChild;
            public static ChildInfo clusterChild;
            public static ChildInfo dynamicTypeChild;
            public static ChildInfo instanceChild;
            public static ChildInfo jointChild;
            public static ChildInfo lightChild;
            public static ChildInfo locatorChild;
            public static ChildInfo lodgroupChild;
            public static ChildInfo meshChild;
            public static ChildInfo multiBlendTargetChild;
            public static ChildInfo nodeChild;
            public static ChildInfo nurbsCurveChild;
            public static ChildInfo nurbsChild;
            public static ChildInfo orientationConstraintChild;
            public static ChildInfo parentConstraintChild;
            public static ChildInfo primitiveChild;
            public static ChildInfo referenceChild;
            public static ChildInfo rigidBodyChild;
            public static ChildInfo scaleConstraintChild;
            public static ChildInfo springConstraintChild;
            public static ChildInfo translationConstraintChild;
            public static ChildInfo animclipChild;
            public static ChildInfo blendChild;
            public static ChildInfo blendshapeControllerChild;
            public static ChildInfo cgshaderChild;
            public static ChildInfo deformerChild;
            public static ChildInfo expressionChild;
            public static ChildInfo imageChild;
            public static ChildInfo materialChild;
            public static ChildInfo motionPathChild;
            public static ChildInfo objsetChild;
            public static ChildInfo pmdataATGChild;
            public static ChildInfo poseChild;
            public static ChildInfo sceneChild;
            public static ChildInfo shaderChild;
            public static ChildInfo textureChild;
            public static ChildInfo blendshapeChild;
            public static ChildInfo skinChild;
            public static ChildInfo customDataChild;
        }

        public static class nurbsSurfaceType_surface
        {
            public static DomNodeType Type;
            public static AttributeInfo uOrderAttribute;
            public static AttributeInfo vOrderAttribute;
            public static AttributeInfo uMinAttribute;
            public static AttributeInfo uMaxAttribute;
            public static AttributeInfo vMinAttribute;
            public static AttributeInfo vMaxAttribute;
            public static ChildInfo controlVerticesChild;
            public static ChildInfo knotsInUChild;
            public static ChildInfo knotsInVChild;
        }

        public static class surface_controlVertices
        {
            public static DomNodeType Type;
            public static AttributeInfo Attribute;
            public static AttributeInfo numCVinUAttribute;
            public static AttributeInfo numCVinVAttribute;
        }

        public static class surface_knotsInU
        {
            public static DomNodeType Type;
            public static AttributeInfo Attribute;
            public static AttributeInfo numKnotsInUAttribute;
        }

        public static class surface_knotsInV
        {
            public static DomNodeType Type;
            public static AttributeInfo Attribute;
            public static AttributeInfo numKnotsInVAttribute;
        }

        public static class orientConstraintType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo constrainAttribute;
            public static AttributeInfo orientInterpolationAttribute;
            public static ChildInfo targetChild;
            public static ChildInfo animChannelChild;
            public static ChildInfo animDiscontinuitiesChild;
            public static ChildInfo animChild;
            public static ChildInfo aimConstraintChild;
            public static ChildInfo atgilocatorChild;
            public static ChildInfo blendtargetChild;
            public static ChildInfo cameraChild;
            public static ChildInfo constraintChild;
            public static ChildInfo clusterChild;
            public static ChildInfo dynamicTypeChild;
            public static ChildInfo instanceChild;
            public static ChildInfo jointChild;
            public static ChildInfo lightChild;
            public static ChildInfo locatorChild;
            public static ChildInfo lodgroupChild;
            public static ChildInfo meshChild;
            public static ChildInfo multiBlendTargetChild;
            public static ChildInfo nodeChild;
            public static ChildInfo nurbsCurveChild;
            public static ChildInfo nurbsChild;
            public static ChildInfo orientationConstraintChild;
            public static ChildInfo parentConstraintChild;
            public static ChildInfo primitiveChild;
            public static ChildInfo referenceChild;
            public static ChildInfo rigidBodyChild;
            public static ChildInfo scaleConstraintChild;
            public static ChildInfo springConstraintChild;
            public static ChildInfo translationConstraintChild;
            public static ChildInfo animclipChild;
            public static ChildInfo blendChild;
            public static ChildInfo blendshapeControllerChild;
            public static ChildInfo cgshaderChild;
            public static ChildInfo deformerChild;
            public static ChildInfo expressionChild;
            public static ChildInfo imageChild;
            public static ChildInfo materialChild;
            public static ChildInfo motionPathChild;
            public static ChildInfo objsetChild;
            public static ChildInfo pmdataATGChild;
            public static ChildInfo poseChild;
            public static ChildInfo sceneChild;
            public static ChildInfo shaderChild;
            public static ChildInfo textureChild;
            public static ChildInfo blendshapeChild;
            public static ChildInfo skinChild;
            public static ChildInfo customDataChild;
            public static ChildInfo rotEulChild;
        }

        public static class parentConstraintType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo constrainAttribute;
            public static ChildInfo offsetChild;
            public static ChildInfo targetChild;
            public static ChildInfo animChannelChild;
            public static ChildInfo animDiscontinuitiesChild;
            public static ChildInfo animChild;
            public static ChildInfo aimConstraintChild;
            public static ChildInfo atgilocatorChild;
            public static ChildInfo blendtargetChild;
            public static ChildInfo cameraChild;
            public static ChildInfo constraintChild;
            public static ChildInfo clusterChild;
            public static ChildInfo dynamicTypeChild;
            public static ChildInfo instanceChild;
            public static ChildInfo jointChild;
            public static ChildInfo lightChild;
            public static ChildInfo locatorChild;
            public static ChildInfo lodgroupChild;
            public static ChildInfo meshChild;
            public static ChildInfo multiBlendTargetChild;
            public static ChildInfo nodeChild;
            public static ChildInfo nurbsCurveChild;
            public static ChildInfo nurbsChild;
            public static ChildInfo orientationConstraintChild;
            public static ChildInfo parentConstraintChild;
            public static ChildInfo primitiveChild;
            public static ChildInfo referenceChild;
            public static ChildInfo rigidBodyChild;
            public static ChildInfo scaleConstraintChild;
            public static ChildInfo springConstraintChild;
            public static ChildInfo translationConstraintChild;
            public static ChildInfo animclipChild;
            public static ChildInfo blendChild;
            public static ChildInfo blendshapeControllerChild;
            public static ChildInfo cgshaderChild;
            public static ChildInfo deformerChild;
            public static ChildInfo expressionChild;
            public static ChildInfo imageChild;
            public static ChildInfo materialChild;
            public static ChildInfo motionPathChild;
            public static ChildInfo objsetChild;
            public static ChildInfo pmdataATGChild;
            public static ChildInfo poseChild;
            public static ChildInfo sceneChild;
            public static ChildInfo shaderChild;
            public static ChildInfo textureChild;
            public static ChildInfo blendshapeChild;
            public static ChildInfo skinChild;
            public static ChildInfo customDataChild;
        }

        public static class parentConstraintType_offset
        {
            public static DomNodeType Type;
            public static AttributeInfo translationAttribute;
            public static ChildInfo rotEulChild;
        }

        public static class primitiveShapeType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo shaderAttribute;
            public static ChildInfo cubeChild;
            public static ChildInfo cylinderChild;
            public static ChildInfo sphereChild;
            public static ChildInfo animChannelChild;
            public static ChildInfo animDiscontinuitiesChild;
            public static ChildInfo animChild;
            public static ChildInfo aimConstraintChild;
            public static ChildInfo atgilocatorChild;
            public static ChildInfo blendtargetChild;
            public static ChildInfo cameraChild;
            public static ChildInfo constraintChild;
            public static ChildInfo clusterChild;
            public static ChildInfo dynamicTypeChild;
            public static ChildInfo instanceChild;
            public static ChildInfo jointChild;
            public static ChildInfo lightChild;
            public static ChildInfo locatorChild;
            public static ChildInfo lodgroupChild;
            public static ChildInfo meshChild;
            public static ChildInfo multiBlendTargetChild;
            public static ChildInfo nodeChild;
            public static ChildInfo nurbsCurveChild;
            public static ChildInfo nurbsChild;
            public static ChildInfo orientationConstraintChild;
            public static ChildInfo parentConstraintChild;
            public static ChildInfo primitiveChild;
            public static ChildInfo referenceChild;
            public static ChildInfo rigidBodyChild;
            public static ChildInfo scaleConstraintChild;
            public static ChildInfo springConstraintChild;
            public static ChildInfo translationConstraintChild;
            public static ChildInfo animclipChild;
            public static ChildInfo blendChild;
            public static ChildInfo blendshapeControllerChild;
            public static ChildInfo cgshaderChild;
            public static ChildInfo deformerChild;
            public static ChildInfo expressionChild;
            public static ChildInfo imageChild;
            public static ChildInfo materialChild;
            public static ChildInfo motionPathChild;
            public static ChildInfo objsetChild;
            public static ChildInfo pmdataATGChild;
            public static ChildInfo poseChild;
            public static ChildInfo sceneChild;
            public static ChildInfo shaderChild;
            public static ChildInfo textureChild;
            public static ChildInfo blendshapeChild;
            public static ChildInfo skinChild;
            public static ChildInfo customDataChild;
        }

        public static class primitiveShapeType_cube
        {
            public static DomNodeType Type;
            public static AttributeInfo widthAttribute;
            public static AttributeInfo heightAttribute;
            public static AttributeInfo depthAttribute;
        }

        public static class primitiveShapeType_cylinder
        {
            public static DomNodeType Type;
            public static AttributeInfo radiusAttribute;
            public static AttributeInfo heightAttribute;
        }

        public static class primitiveShapeType_sphere
        {
            public static DomNodeType Type;
            public static AttributeInfo radiusAttribute;
        }

        public static class referenceType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo uriAttribute;
            public static AttributeInfo namespaceAttribute;
            public static ChildInfo animChannelChild;
            public static ChildInfo animDiscontinuitiesChild;
            public static ChildInfo animChild;
            public static ChildInfo aimConstraintChild;
            public static ChildInfo atgilocatorChild;
            public static ChildInfo blendtargetChild;
            public static ChildInfo cameraChild;
            public static ChildInfo constraintChild;
            public static ChildInfo clusterChild;
            public static ChildInfo dynamicTypeChild;
            public static ChildInfo instanceChild;
            public static ChildInfo jointChild;
            public static ChildInfo lightChild;
            public static ChildInfo locatorChild;
            public static ChildInfo lodgroupChild;
            public static ChildInfo meshChild;
            public static ChildInfo multiBlendTargetChild;
            public static ChildInfo nodeChild;
            public static ChildInfo nurbsCurveChild;
            public static ChildInfo nurbsChild;
            public static ChildInfo orientationConstraintChild;
            public static ChildInfo parentConstraintChild;
            public static ChildInfo primitiveChild;
            public static ChildInfo referenceChild;
            public static ChildInfo rigidBodyChild;
            public static ChildInfo scaleConstraintChild;
            public static ChildInfo springConstraintChild;
            public static ChildInfo translationConstraintChild;
            public static ChildInfo animclipChild;
            public static ChildInfo blendChild;
            public static ChildInfo blendshapeControllerChild;
            public static ChildInfo cgshaderChild;
            public static ChildInfo deformerChild;
            public static ChildInfo expressionChild;
            public static ChildInfo imageChild;
            public static ChildInfo materialChild;
            public static ChildInfo motionPathChild;
            public static ChildInfo objsetChild;
            public static ChildInfo pmdataATGChild;
            public static ChildInfo poseChild;
            public static ChildInfo sceneChild;
            public static ChildInfo shaderChild;
            public static ChildInfo textureChild;
            public static ChildInfo blendshapeChild;
            public static ChildInfo skinChild;
            public static ChildInfo customDataChild;
        }

        public static class rigidBodyType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo massAttribute;
            public static AttributeInfo geometryAttribute;
            public static ChildInfo animChannelChild;
            public static ChildInfo animDiscontinuitiesChild;
            public static ChildInfo animChild;
            public static ChildInfo aimConstraintChild;
            public static ChildInfo atgilocatorChild;
            public static ChildInfo blendtargetChild;
            public static ChildInfo cameraChild;
            public static ChildInfo constraintChild;
            public static ChildInfo clusterChild;
            public static ChildInfo dynamicTypeChild;
            public static ChildInfo instanceChild;
            public static ChildInfo jointChild;
            public static ChildInfo lightChild;
            public static ChildInfo locatorChild;
            public static ChildInfo lodgroupChild;
            public static ChildInfo meshChild;
            public static ChildInfo multiBlendTargetChild;
            public static ChildInfo nodeChild;
            public static ChildInfo nurbsCurveChild;
            public static ChildInfo nurbsChild;
            public static ChildInfo orientationConstraintChild;
            public static ChildInfo parentConstraintChild;
            public static ChildInfo primitiveChild;
            public static ChildInfo referenceChild;
            public static ChildInfo rigidBodyChild;
            public static ChildInfo scaleConstraintChild;
            public static ChildInfo springConstraintChild;
            public static ChildInfo translationConstraintChild;
            public static ChildInfo animclipChild;
            public static ChildInfo blendChild;
            public static ChildInfo blendshapeControllerChild;
            public static ChildInfo cgshaderChild;
            public static ChildInfo deformerChild;
            public static ChildInfo expressionChild;
            public static ChildInfo imageChild;
            public static ChildInfo materialChild;
            public static ChildInfo motionPathChild;
            public static ChildInfo objsetChild;
            public static ChildInfo pmdataATGChild;
            public static ChildInfo poseChild;
            public static ChildInfo sceneChild;
            public static ChildInfo shaderChild;
            public static ChildInfo textureChild;
            public static ChildInfo blendshapeChild;
            public static ChildInfo skinChild;
            public static ChildInfo customDataChild;
        }

        public static class scaleConstraintType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo offsetAttribute;
            public static AttributeInfo constrainAttribute;
            public static ChildInfo targetChild;
            public static ChildInfo animChannelChild;
            public static ChildInfo animDiscontinuitiesChild;
            public static ChildInfo animChild;
            public static ChildInfo aimConstraintChild;
            public static ChildInfo atgilocatorChild;
            public static ChildInfo blendtargetChild;
            public static ChildInfo cameraChild;
            public static ChildInfo constraintChild;
            public static ChildInfo clusterChild;
            public static ChildInfo dynamicTypeChild;
            public static ChildInfo instanceChild;
            public static ChildInfo jointChild;
            public static ChildInfo lightChild;
            public static ChildInfo locatorChild;
            public static ChildInfo lodgroupChild;
            public static ChildInfo meshChild;
            public static ChildInfo multiBlendTargetChild;
            public static ChildInfo nodeChild;
            public static ChildInfo nurbsCurveChild;
            public static ChildInfo nurbsChild;
            public static ChildInfo orientationConstraintChild;
            public static ChildInfo parentConstraintChild;
            public static ChildInfo primitiveChild;
            public static ChildInfo referenceChild;
            public static ChildInfo rigidBodyChild;
            public static ChildInfo scaleConstraintChild;
            public static ChildInfo springConstraintChild;
            public static ChildInfo translationConstraintChild;
            public static ChildInfo animclipChild;
            public static ChildInfo blendChild;
            public static ChildInfo blendshapeControllerChild;
            public static ChildInfo cgshaderChild;
            public static ChildInfo deformerChild;
            public static ChildInfo expressionChild;
            public static ChildInfo imageChild;
            public static ChildInfo materialChild;
            public static ChildInfo motionPathChild;
            public static ChildInfo objsetChild;
            public static ChildInfo pmdataATGChild;
            public static ChildInfo poseChild;
            public static ChildInfo sceneChild;
            public static ChildInfo shaderChild;
            public static ChildInfo textureChild;
            public static ChildInfo blendshapeChild;
            public static ChildInfo skinChild;
            public static ChildInfo customDataChild;
        }

        public static class springConstraintType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo stiffnessAttribute;
            public static AttributeInfo restLengthAttribute;
            public static AttributeInfo dampingAttribute;
            public static AttributeInfo constrainAttribute;
            public static ChildInfo targetChild;
            public static ChildInfo animChannelChild;
            public static ChildInfo animDiscontinuitiesChild;
            public static ChildInfo animChild;
            public static ChildInfo aimConstraintChild;
            public static ChildInfo atgilocatorChild;
            public static ChildInfo blendtargetChild;
            public static ChildInfo cameraChild;
            public static ChildInfo constraintChild;
            public static ChildInfo clusterChild;
            public static ChildInfo dynamicTypeChild;
            public static ChildInfo instanceChild;
            public static ChildInfo jointChild;
            public static ChildInfo lightChild;
            public static ChildInfo locatorChild;
            public static ChildInfo lodgroupChild;
            public static ChildInfo meshChild;
            public static ChildInfo multiBlendTargetChild;
            public static ChildInfo nodeChild;
            public static ChildInfo nurbsCurveChild;
            public static ChildInfo nurbsChild;
            public static ChildInfo orientationConstraintChild;
            public static ChildInfo parentConstraintChild;
            public static ChildInfo primitiveChild;
            public static ChildInfo referenceChild;
            public static ChildInfo rigidBodyChild;
            public static ChildInfo scaleConstraintChild;
            public static ChildInfo springConstraintChild;
            public static ChildInfo translationConstraintChild;
            public static ChildInfo animclipChild;
            public static ChildInfo blendChild;
            public static ChildInfo blendshapeControllerChild;
            public static ChildInfo cgshaderChild;
            public static ChildInfo deformerChild;
            public static ChildInfo expressionChild;
            public static ChildInfo imageChild;
            public static ChildInfo materialChild;
            public static ChildInfo motionPathChild;
            public static ChildInfo objsetChild;
            public static ChildInfo pmdataATGChild;
            public static ChildInfo poseChild;
            public static ChildInfo sceneChild;
            public static ChildInfo shaderChild;
            public static ChildInfo textureChild;
            public static ChildInfo blendshapeChild;
            public static ChildInfo skinChild;
            public static ChildInfo customDataChild;
        }

        public static class translationConstraintType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo offsetAttribute;
            public static AttributeInfo constrainAttribute;
            public static ChildInfo targetChild;
            public static ChildInfo animChannelChild;
            public static ChildInfo animDiscontinuitiesChild;
            public static ChildInfo animChild;
            public static ChildInfo aimConstraintChild;
            public static ChildInfo atgilocatorChild;
            public static ChildInfo blendtargetChild;
            public static ChildInfo cameraChild;
            public static ChildInfo constraintChild;
            public static ChildInfo clusterChild;
            public static ChildInfo dynamicTypeChild;
            public static ChildInfo instanceChild;
            public static ChildInfo jointChild;
            public static ChildInfo lightChild;
            public static ChildInfo locatorChild;
            public static ChildInfo lodgroupChild;
            public static ChildInfo meshChild;
            public static ChildInfo multiBlendTargetChild;
            public static ChildInfo nodeChild;
            public static ChildInfo nurbsCurveChild;
            public static ChildInfo nurbsChild;
            public static ChildInfo orientationConstraintChild;
            public static ChildInfo parentConstraintChild;
            public static ChildInfo primitiveChild;
            public static ChildInfo referenceChild;
            public static ChildInfo rigidBodyChild;
            public static ChildInfo scaleConstraintChild;
            public static ChildInfo springConstraintChild;
            public static ChildInfo translationConstraintChild;
            public static ChildInfo animclipChild;
            public static ChildInfo blendChild;
            public static ChildInfo blendshapeControllerChild;
            public static ChildInfo cgshaderChild;
            public static ChildInfo deformerChild;
            public static ChildInfo expressionChild;
            public static ChildInfo imageChild;
            public static ChildInfo materialChild;
            public static ChildInfo motionPathChild;
            public static ChildInfo objsetChild;
            public static ChildInfo pmdataATGChild;
            public static ChildInfo poseChild;
            public static ChildInfo sceneChild;
            public static ChildInfo shaderChild;
            public static ChildInfo textureChild;
            public static ChildInfo blendshapeChild;
            public static ChildInfo skinChild;
            public static ChildInfo customDataChild;
        }

        public static class animclipType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static ChildInfo animChannelChild;
            public static ChildInfo animDiscontinuitiesChild;
            public static ChildInfo animChild;
            public static ChildInfo aimConstraintChild;
            public static ChildInfo atgilocatorChild;
            public static ChildInfo blendtargetChild;
            public static ChildInfo cameraChild;
            public static ChildInfo constraintChild;
            public static ChildInfo clusterChild;
            public static ChildInfo dynamicTypeChild;
            public static ChildInfo instanceChild;
            public static ChildInfo jointChild;
            public static ChildInfo lightChild;
            public static ChildInfo locatorChild;
            public static ChildInfo lodgroupChild;
            public static ChildInfo meshChild;
            public static ChildInfo multiBlendTargetChild;
            public static ChildInfo nodeChild;
            public static ChildInfo nurbsCurveChild;
            public static ChildInfo nurbsChild;
            public static ChildInfo orientationConstraintChild;
            public static ChildInfo parentConstraintChild;
            public static ChildInfo primitiveChild;
            public static ChildInfo referenceChild;
            public static ChildInfo rigidBodyChild;
            public static ChildInfo scaleConstraintChild;
            public static ChildInfo springConstraintChild;
            public static ChildInfo translationConstraintChild;
            public static ChildInfo animclipChild;
            public static ChildInfo blendChild;
            public static ChildInfo blendshapeControllerChild;
            public static ChildInfo cgshaderChild;
            public static ChildInfo deformerChild;
            public static ChildInfo expressionChild;
            public static ChildInfo imageChild;
            public static ChildInfo materialChild;
            public static ChildInfo motionPathChild;
            public static ChildInfo objsetChild;
            public static ChildInfo pmdataATGChild;
            public static ChildInfo poseChild;
            public static ChildInfo sceneChild;
            public static ChildInfo shaderChild;
            public static ChildInfo textureChild;
            public static ChildInfo blendshapeChild;
            public static ChildInfo skinChild;
            public static ChildInfo customDataChild;
        }

        public static class blendType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo targetAttribute;
            public static AttributeInfo channelAttribute;
            public static ChildInfo inputChild;
            public static ChildInfo animChannelChild;
            public static ChildInfo animDiscontinuitiesChild;
            public static ChildInfo animChild;
            public static ChildInfo aimConstraintChild;
            public static ChildInfo atgilocatorChild;
            public static ChildInfo blendtargetChild;
            public static ChildInfo cameraChild;
            public static ChildInfo constraintChild;
            public static ChildInfo clusterChild;
            public static ChildInfo dynamicTypeChild;
            public static ChildInfo instanceChild;
            public static ChildInfo jointChild;
            public static ChildInfo lightChild;
            public static ChildInfo locatorChild;
            public static ChildInfo lodgroupChild;
            public static ChildInfo meshChild;
            public static ChildInfo multiBlendTargetChild;
            public static ChildInfo nodeChild;
            public static ChildInfo nurbsCurveChild;
            public static ChildInfo nurbsChild;
            public static ChildInfo orientationConstraintChild;
            public static ChildInfo parentConstraintChild;
            public static ChildInfo primitiveChild;
            public static ChildInfo referenceChild;
            public static ChildInfo rigidBodyChild;
            public static ChildInfo scaleConstraintChild;
            public static ChildInfo springConstraintChild;
            public static ChildInfo translationConstraintChild;
            public static ChildInfo animclipChild;
            public static ChildInfo blendChild;
            public static ChildInfo blendshapeControllerChild;
            public static ChildInfo cgshaderChild;
            public static ChildInfo deformerChild;
            public static ChildInfo expressionChild;
            public static ChildInfo imageChild;
            public static ChildInfo materialChild;
            public static ChildInfo motionPathChild;
            public static ChildInfo objsetChild;
            public static ChildInfo pmdataATGChild;
            public static ChildInfo poseChild;
            public static ChildInfo sceneChild;
            public static ChildInfo shaderChild;
            public static ChildInfo textureChild;
            public static ChildInfo blendshapeChild;
            public static ChildInfo skinChild;
            public static ChildInfo customDataChild;
        }

        public static class blendType_input
        {
            public static DomNodeType Type;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo weightAttribute;
        }

        public static class blendshapeControllerType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static ChildInfo weightChild;
            public static ChildInfo animChannelChild;
            public static ChildInfo animDiscontinuitiesChild;
            public static ChildInfo animChild;
            public static ChildInfo aimConstraintChild;
            public static ChildInfo atgilocatorChild;
            public static ChildInfo blendtargetChild;
            public static ChildInfo cameraChild;
            public static ChildInfo constraintChild;
            public static ChildInfo clusterChild;
            public static ChildInfo dynamicTypeChild;
            public static ChildInfo instanceChild;
            public static ChildInfo jointChild;
            public static ChildInfo lightChild;
            public static ChildInfo locatorChild;
            public static ChildInfo lodgroupChild;
            public static ChildInfo meshChild;
            public static ChildInfo multiBlendTargetChild;
            public static ChildInfo nodeChild;
            public static ChildInfo nurbsCurveChild;
            public static ChildInfo nurbsChild;
            public static ChildInfo orientationConstraintChild;
            public static ChildInfo parentConstraintChild;
            public static ChildInfo primitiveChild;
            public static ChildInfo referenceChild;
            public static ChildInfo rigidBodyChild;
            public static ChildInfo scaleConstraintChild;
            public static ChildInfo springConstraintChild;
            public static ChildInfo translationConstraintChild;
            public static ChildInfo animclipChild;
            public static ChildInfo blendChild;
            public static ChildInfo blendshapeControllerChild;
            public static ChildInfo cgshaderChild;
            public static ChildInfo deformerChild;
            public static ChildInfo expressionChild;
            public static ChildInfo imageChild;
            public static ChildInfo materialChild;
            public static ChildInfo motionPathChild;
            public static ChildInfo objsetChild;
            public static ChildInfo pmdataATGChild;
            public static ChildInfo poseChild;
            public static ChildInfo sceneChild;
            public static ChildInfo shaderChild;
            public static ChildInfo textureChild;
            public static ChildInfo blendshapeChild;
            public static ChildInfo skinChild;
            public static ChildInfo customDataChild;
        }

        public static class blendshapeControllerType_weight
        {
            public static DomNodeType Type;
            public static AttributeInfo weightNameAttribute;
        }

        public static class cgshaderType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo urlAttribute;
            public static ChildInfo bindingChild;
            public static ChildInfo animChannelChild;
            public static ChildInfo animDiscontinuitiesChild;
            public static ChildInfo animChild;
            public static ChildInfo aimConstraintChild;
            public static ChildInfo atgilocatorChild;
            public static ChildInfo blendtargetChild;
            public static ChildInfo cameraChild;
            public static ChildInfo constraintChild;
            public static ChildInfo clusterChild;
            public static ChildInfo dynamicTypeChild;
            public static ChildInfo instanceChild;
            public static ChildInfo jointChild;
            public static ChildInfo lightChild;
            public static ChildInfo locatorChild;
            public static ChildInfo lodgroupChild;
            public static ChildInfo meshChild;
            public static ChildInfo multiBlendTargetChild;
            public static ChildInfo nodeChild;
            public static ChildInfo nurbsCurveChild;
            public static ChildInfo nurbsChild;
            public static ChildInfo orientationConstraintChild;
            public static ChildInfo parentConstraintChild;
            public static ChildInfo primitiveChild;
            public static ChildInfo referenceChild;
            public static ChildInfo rigidBodyChild;
            public static ChildInfo scaleConstraintChild;
            public static ChildInfo springConstraintChild;
            public static ChildInfo translationConstraintChild;
            public static ChildInfo animclipChild;
            public static ChildInfo blendChild;
            public static ChildInfo blendshapeControllerChild;
            public static ChildInfo cgshaderChild;
            public static ChildInfo deformerChild;
            public static ChildInfo expressionChild;
            public static ChildInfo imageChild;
            public static ChildInfo materialChild;
            public static ChildInfo motionPathChild;
            public static ChildInfo objsetChild;
            public static ChildInfo pmdataATGChild;
            public static ChildInfo poseChild;
            public static ChildInfo sceneChild;
            public static ChildInfo shaderChild;
            public static ChildInfo textureChild;
            public static ChildInfo blendshapeChild;
            public static ChildInfo skinChild;
            public static ChildInfo customDataChild;
        }

        public static class cgshaderType_binding
        {
            public static DomNodeType Type;
            public static AttributeInfo Attribute;
            public static AttributeInfo tagAttribute;
            public static AttributeInfo typeAttribute;
            public static AttributeInfo sourceAttribute;
            public static AttributeInfo datasetAttribute;
            public static AttributeInfo countAttribute;
        }

        public static class deformerType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo targetAttribute;
            public static ChildInfo animChannelChild;
            public static ChildInfo animDiscontinuitiesChild;
            public static ChildInfo animChild;
            public static ChildInfo aimConstraintChild;
            public static ChildInfo atgilocatorChild;
            public static ChildInfo blendtargetChild;
            public static ChildInfo cameraChild;
            public static ChildInfo constraintChild;
            public static ChildInfo clusterChild;
            public static ChildInfo dynamicTypeChild;
            public static ChildInfo instanceChild;
            public static ChildInfo jointChild;
            public static ChildInfo lightChild;
            public static ChildInfo locatorChild;
            public static ChildInfo lodgroupChild;
            public static ChildInfo meshChild;
            public static ChildInfo multiBlendTargetChild;
            public static ChildInfo nodeChild;
            public static ChildInfo nurbsCurveChild;
            public static ChildInfo nurbsChild;
            public static ChildInfo orientationConstraintChild;
            public static ChildInfo parentConstraintChild;
            public static ChildInfo primitiveChild;
            public static ChildInfo referenceChild;
            public static ChildInfo rigidBodyChild;
            public static ChildInfo scaleConstraintChild;
            public static ChildInfo springConstraintChild;
            public static ChildInfo translationConstraintChild;
            public static ChildInfo animclipChild;
            public static ChildInfo blendChild;
            public static ChildInfo blendshapeControllerChild;
            public static ChildInfo cgshaderChild;
            public static ChildInfo deformerChild;
            public static ChildInfo expressionChild;
            public static ChildInfo imageChild;
            public static ChildInfo materialChild;
            public static ChildInfo motionPathChild;
            public static ChildInfo objsetChild;
            public static ChildInfo pmdataATGChild;
            public static ChildInfo poseChild;
            public static ChildInfo sceneChild;
            public static ChildInfo shaderChild;
            public static ChildInfo textureChild;
            public static ChildInfo blendshapeChild;
            public static ChildInfo skinChild;
            public static ChildInfo customDataChild;
        }

        public static class expressionType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo codeAttribute;
            public static ChildInfo inputChild;
            public static ChildInfo outputChild;
            public static ChildInfo animChannelChild;
            public static ChildInfo animDiscontinuitiesChild;
            public static ChildInfo animChild;
            public static ChildInfo aimConstraintChild;
            public static ChildInfo atgilocatorChild;
            public static ChildInfo blendtargetChild;
            public static ChildInfo cameraChild;
            public static ChildInfo constraintChild;
            public static ChildInfo clusterChild;
            public static ChildInfo dynamicTypeChild;
            public static ChildInfo instanceChild;
            public static ChildInfo jointChild;
            public static ChildInfo lightChild;
            public static ChildInfo locatorChild;
            public static ChildInfo lodgroupChild;
            public static ChildInfo meshChild;
            public static ChildInfo multiBlendTargetChild;
            public static ChildInfo nodeChild;
            public static ChildInfo nurbsCurveChild;
            public static ChildInfo nurbsChild;
            public static ChildInfo orientationConstraintChild;
            public static ChildInfo parentConstraintChild;
            public static ChildInfo primitiveChild;
            public static ChildInfo referenceChild;
            public static ChildInfo rigidBodyChild;
            public static ChildInfo scaleConstraintChild;
            public static ChildInfo springConstraintChild;
            public static ChildInfo translationConstraintChild;
            public static ChildInfo animclipChild;
            public static ChildInfo blendChild;
            public static ChildInfo blendshapeControllerChild;
            public static ChildInfo cgshaderChild;
            public static ChildInfo deformerChild;
            public static ChildInfo expressionChild;
            public static ChildInfo imageChild;
            public static ChildInfo materialChild;
            public static ChildInfo motionPathChild;
            public static ChildInfo objsetChild;
            public static ChildInfo pmdataATGChild;
            public static ChildInfo poseChild;
            public static ChildInfo sceneChild;
            public static ChildInfo shaderChild;
            public static ChildInfo textureChild;
            public static ChildInfo blendshapeChild;
            public static ChildInfo skinChild;
            public static ChildInfo customDataChild;
        }

        public static class expressionType_input
        {
            public static DomNodeType Type;
            public static AttributeInfo objectAttribute;
            public static AttributeInfo channelAttribute;
        }

        public static class expressionType_output
        {
            public static DomNodeType Type;
            public static AttributeInfo objectAttribute;
            public static AttributeInfo channelAttribute;
        }

        public static class imageType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo widthAttribute;
            public static AttributeInfo heightAttribute;
            public static ChildInfo dataChild;
            public static ChildInfo image_channelChild;
        }

        public static class imageType_data
        {
            public static DomNodeType Type;
            public static AttributeInfo widthAttribute;
            public static AttributeInfo heightAttribute;
        }

        public static class image_channelType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static ChildInfo dataChild;
        }

        public static class image_channelType_data
        {
            public static DomNodeType Type;
            public static AttributeInfo Attribute;
            public static AttributeInfo componentCountAttribute;
            public static AttributeInfo widthAttribute;
            public static AttributeInfo heightAttribute;
        }

        public static class materialType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo urlAttribute;
            public static AttributeInfo matAttribute;
            public static AttributeInfo frompresetAttribute;
            public static AttributeInfo preseturlAttribute;
            public static AttributeInfo presetAttribute;
            public static ChildInfo renderstateChild;
            public static ChildInfo bindingChild;
            public static ChildInfo animChannelChild;
            public static ChildInfo animDiscontinuitiesChild;
            public static ChildInfo animChild;
            public static ChildInfo aimConstraintChild;
            public static ChildInfo atgilocatorChild;
            public static ChildInfo blendtargetChild;
            public static ChildInfo cameraChild;
            public static ChildInfo constraintChild;
            public static ChildInfo clusterChild;
            public static ChildInfo dynamicTypeChild;
            public static ChildInfo instanceChild;
            public static ChildInfo jointChild;
            public static ChildInfo lightChild;
            public static ChildInfo locatorChild;
            public static ChildInfo lodgroupChild;
            public static ChildInfo meshChild;
            public static ChildInfo multiBlendTargetChild;
            public static ChildInfo nodeChild;
            public static ChildInfo nurbsCurveChild;
            public static ChildInfo nurbsChild;
            public static ChildInfo orientationConstraintChild;
            public static ChildInfo parentConstraintChild;
            public static ChildInfo primitiveChild;
            public static ChildInfo referenceChild;
            public static ChildInfo rigidBodyChild;
            public static ChildInfo scaleConstraintChild;
            public static ChildInfo springConstraintChild;
            public static ChildInfo translationConstraintChild;
            public static ChildInfo animclipChild;
            public static ChildInfo blendChild;
            public static ChildInfo blendshapeControllerChild;
            public static ChildInfo cgshaderChild;
            public static ChildInfo deformerChild;
            public static ChildInfo expressionChild;
            public static ChildInfo imageChild;
            public static ChildInfo materialChild;
            public static ChildInfo motionPathChild;
            public static ChildInfo objsetChild;
            public static ChildInfo pmdataATGChild;
            public static ChildInfo poseChild;
            public static ChildInfo sceneChild;
            public static ChildInfo shaderChild;
            public static ChildInfo textureChild;
            public static ChildInfo blendshapeChild;
            public static ChildInfo skinChild;
            public static ChildInfo customDataChild;
        }

        public static class materialType_renderstate
        {
            public static DomNodeType Type;
            public static AttributeInfo overridenAttribute;
            public static ChildInfo alphablendChild;
            public static ChildInfo alphatestChild;
            public static ChildInfo zwriteChild;
            public static ChildInfo ztestChild;
            public static ChildInfo backfacecullingChild;
        }

        public static class renderstate_alphablend
        {
            public static DomNodeType Type;
            public static AttributeInfo enabledAttribute;
            public static AttributeInfo sourceblendAttribute;
            public static AttributeInfo destblendAttribute;
        }

        public static class renderstate_alphatest
        {
            public static DomNodeType Type;
            public static AttributeInfo enabledAttribute;
            public static AttributeInfo alphafuncAttribute;
            public static AttributeInfo alpharefAttribute;
        }

        public static class renderstate_zwrite
        {
            public static DomNodeType Type;
            public static AttributeInfo enabledAttribute;
        }

        public static class renderstate_ztest
        {
            public static DomNodeType Type;
            public static AttributeInfo enabledAttribute;
            public static AttributeInfo ztestfuncAttribute;
        }

        public static class renderstate_backfaceculling
        {
            public static DomNodeType Type;
            public static AttributeInfo enabledAttribute;
        }

        public static class materialType_binding
        {
            public static DomNodeType Type;
            public static AttributeInfo Attribute;
            public static AttributeInfo tagAttribute;
            public static AttributeInfo typeAttribute;
            public static AttributeInfo datasetAttribute;
            public static AttributeInfo countAttribute;
            public static AttributeInfo sourceAttribute;
        }

        public static class motionPathType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo aimAttribute;
            public static AttributeInfo upAttribute;
            public static AttributeInfo globalupAttribute;
            public static AttributeInfo constrainAttribute;
            public static AttributeInfo parameterisationAttribute;
            public static AttributeInfo constrainOrientationAttribute;
            public static AttributeInfo curvePathAttribute;
            public static ChildInfo offsetChild;
            public static ChildInfo upobjectChild;
            public static ChildInfo targetChild;
            public static ChildInfo animChannelChild;
            public static ChildInfo animDiscontinuitiesChild;
            public static ChildInfo animChild;
            public static ChildInfo aimConstraintChild;
            public static ChildInfo atgilocatorChild;
            public static ChildInfo blendtargetChild;
            public static ChildInfo cameraChild;
            public static ChildInfo constraintChild;
            public static ChildInfo clusterChild;
            public static ChildInfo dynamicTypeChild;
            public static ChildInfo instanceChild;
            public static ChildInfo jointChild;
            public static ChildInfo lightChild;
            public static ChildInfo locatorChild;
            public static ChildInfo lodgroupChild;
            public static ChildInfo meshChild;
            public static ChildInfo multiBlendTargetChild;
            public static ChildInfo nodeChild;
            public static ChildInfo nurbsCurveChild;
            public static ChildInfo nurbsChild;
            public static ChildInfo orientationConstraintChild;
            public static ChildInfo parentConstraintChild;
            public static ChildInfo primitiveChild;
            public static ChildInfo referenceChild;
            public static ChildInfo rigidBodyChild;
            public static ChildInfo scaleConstraintChild;
            public static ChildInfo springConstraintChild;
            public static ChildInfo translationConstraintChild;
            public static ChildInfo animclipChild;
            public static ChildInfo blendChild;
            public static ChildInfo blendshapeControllerChild;
            public static ChildInfo cgshaderChild;
            public static ChildInfo deformerChild;
            public static ChildInfo expressionChild;
            public static ChildInfo imageChild;
            public static ChildInfo materialChild;
            public static ChildInfo motionPathChild;
            public static ChildInfo objsetChild;
            public static ChildInfo pmdataATGChild;
            public static ChildInfo poseChild;
            public static ChildInfo sceneChild;
            public static ChildInfo shaderChild;
            public static ChildInfo textureChild;
            public static ChildInfo blendshapeChild;
            public static ChildInfo skinChild;
            public static ChildInfo customDataChild;
        }

        public static class motionPathType_upobject
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo transformAttribute;
        }

        public static class objSetType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo typeAttribute;
            public static ChildInfo memberChild;
            public static ChildInfo animChannelChild;
            public static ChildInfo animDiscontinuitiesChild;
            public static ChildInfo animChild;
            public static ChildInfo aimConstraintChild;
            public static ChildInfo atgilocatorChild;
            public static ChildInfo blendtargetChild;
            public static ChildInfo cameraChild;
            public static ChildInfo constraintChild;
            public static ChildInfo clusterChild;
            public static ChildInfo dynamicTypeChild;
            public static ChildInfo instanceChild;
            public static ChildInfo jointChild;
            public static ChildInfo lightChild;
            public static ChildInfo locatorChild;
            public static ChildInfo lodgroupChild;
            public static ChildInfo meshChild;
            public static ChildInfo multiBlendTargetChild;
            public static ChildInfo nodeChild;
            public static ChildInfo nurbsCurveChild;
            public static ChildInfo nurbsChild;
            public static ChildInfo orientationConstraintChild;
            public static ChildInfo parentConstraintChild;
            public static ChildInfo primitiveChild;
            public static ChildInfo referenceChild;
            public static ChildInfo rigidBodyChild;
            public static ChildInfo scaleConstraintChild;
            public static ChildInfo springConstraintChild;
            public static ChildInfo translationConstraintChild;
            public static ChildInfo animclipChild;
            public static ChildInfo blendChild;
            public static ChildInfo blendshapeControllerChild;
            public static ChildInfo cgshaderChild;
            public static ChildInfo deformerChild;
            public static ChildInfo expressionChild;
            public static ChildInfo imageChild;
            public static ChildInfo materialChild;
            public static ChildInfo motionPathChild;
            public static ChildInfo objsetChild;
            public static ChildInfo pmdataATGChild;
            public static ChildInfo poseChild;
            public static ChildInfo sceneChild;
            public static ChildInfo shaderChild;
            public static ChildInfo textureChild;
            public static ChildInfo blendshapeChild;
            public static ChildInfo skinChild;
            public static ChildInfo customDataChild;
        }

        public static class objSetType_member
        {
            public static DomNodeType Type;
            public static AttributeInfo pathAttribute;
            public static AttributeInfo membershipAttribute;
        }

        public static class pmdataATGType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo vsAttribute;
            public static AttributeInfo vtAttribute;
            public static AttributeInfo facesAttribute;
            public static AttributeInfo costsAttribute;
            public static AttributeInfo splitsAttribute;
            public static AttributeInfo fixFacesAttribute;
            public static AttributeInfo targetAttribute;
            public static ChildInfo animChannelChild;
            public static ChildInfo animDiscontinuitiesChild;
            public static ChildInfo animChild;
            public static ChildInfo aimConstraintChild;
            public static ChildInfo atgilocatorChild;
            public static ChildInfo blendtargetChild;
            public static ChildInfo cameraChild;
            public static ChildInfo constraintChild;
            public static ChildInfo clusterChild;
            public static ChildInfo dynamicTypeChild;
            public static ChildInfo instanceChild;
            public static ChildInfo jointChild;
            public static ChildInfo lightChild;
            public static ChildInfo locatorChild;
            public static ChildInfo lodgroupChild;
            public static ChildInfo meshChild;
            public static ChildInfo multiBlendTargetChild;
            public static ChildInfo nodeChild;
            public static ChildInfo nurbsCurveChild;
            public static ChildInfo nurbsChild;
            public static ChildInfo orientationConstraintChild;
            public static ChildInfo parentConstraintChild;
            public static ChildInfo primitiveChild;
            public static ChildInfo referenceChild;
            public static ChildInfo rigidBodyChild;
            public static ChildInfo scaleConstraintChild;
            public static ChildInfo springConstraintChild;
            public static ChildInfo translationConstraintChild;
            public static ChildInfo animclipChild;
            public static ChildInfo blendChild;
            public static ChildInfo blendshapeControllerChild;
            public static ChildInfo cgshaderChild;
            public static ChildInfo deformerChild;
            public static ChildInfo expressionChild;
            public static ChildInfo imageChild;
            public static ChildInfo materialChild;
            public static ChildInfo motionPathChild;
            public static ChildInfo objsetChild;
            public static ChildInfo pmdataATGChild;
            public static ChildInfo poseChild;
            public static ChildInfo sceneChild;
            public static ChildInfo shaderChild;
            public static ChildInfo textureChild;
            public static ChildInfo blendshapeChild;
            public static ChildInfo skinChild;
            public static ChildInfo customDataChild;
        }

        public static class poseType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo bindPoseAttribute;
            public static ChildInfo elementChild;
            public static ChildInfo animChannelChild;
            public static ChildInfo animDiscontinuitiesChild;
            public static ChildInfo animChild;
            public static ChildInfo aimConstraintChild;
            public static ChildInfo atgilocatorChild;
            public static ChildInfo blendtargetChild;
            public static ChildInfo cameraChild;
            public static ChildInfo constraintChild;
            public static ChildInfo clusterChild;
            public static ChildInfo dynamicTypeChild;
            public static ChildInfo instanceChild;
            public static ChildInfo jointChild;
            public static ChildInfo lightChild;
            public static ChildInfo locatorChild;
            public static ChildInfo lodgroupChild;
            public static ChildInfo meshChild;
            public static ChildInfo multiBlendTargetChild;
            public static ChildInfo nodeChild;
            public static ChildInfo nurbsCurveChild;
            public static ChildInfo nurbsChild;
            public static ChildInfo orientationConstraintChild;
            public static ChildInfo parentConstraintChild;
            public static ChildInfo primitiveChild;
            public static ChildInfo referenceChild;
            public static ChildInfo rigidBodyChild;
            public static ChildInfo scaleConstraintChild;
            public static ChildInfo springConstraintChild;
            public static ChildInfo translationConstraintChild;
            public static ChildInfo animclipChild;
            public static ChildInfo blendChild;
            public static ChildInfo blendshapeControllerChild;
            public static ChildInfo cgshaderChild;
            public static ChildInfo deformerChild;
            public static ChildInfo expressionChild;
            public static ChildInfo imageChild;
            public static ChildInfo materialChild;
            public static ChildInfo motionPathChild;
            public static ChildInfo objsetChild;
            public static ChildInfo pmdataATGChild;
            public static ChildInfo poseChild;
            public static ChildInfo sceneChild;
            public static ChildInfo shaderChild;
            public static ChildInfo textureChild;
            public static ChildInfo blendshapeChild;
            public static ChildInfo skinChild;
            public static ChildInfo customDataChild;
        }

        public static class poseType_element
        {
            public static DomNodeType Type;
            public static AttributeInfo translateAttribute;
            public static AttributeInfo scaleAttribute;
            public static AttributeInfo targetAttribute;
            public static ChildInfo rotEulChild;
        }

        public static class element_rotEul
        {
            public static DomNodeType Type;
            public static AttributeInfo Attribute;
            public static AttributeInfo rotOrdAttribute;
        }

        public static class sceneType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static ChildInfo animChannelChild;
            public static ChildInfo animDiscontinuitiesChild;
            public static ChildInfo animChild;
            public static ChildInfo aimConstraintChild;
            public static ChildInfo atgilocatorChild;
            public static ChildInfo blendtargetChild;
            public static ChildInfo cameraChild;
            public static ChildInfo constraintChild;
            public static ChildInfo clusterChild;
            public static ChildInfo dynamicTypeChild;
            public static ChildInfo instanceChild;
            public static ChildInfo jointChild;
            public static ChildInfo lightChild;
            public static ChildInfo locatorChild;
            public static ChildInfo lodgroupChild;
            public static ChildInfo meshChild;
            public static ChildInfo multiBlendTargetChild;
            public static ChildInfo nodeChild;
            public static ChildInfo nurbsCurveChild;
            public static ChildInfo nurbsChild;
            public static ChildInfo orientationConstraintChild;
            public static ChildInfo parentConstraintChild;
            public static ChildInfo primitiveChild;
            public static ChildInfo referenceChild;
            public static ChildInfo rigidBodyChild;
            public static ChildInfo scaleConstraintChild;
            public static ChildInfo springConstraintChild;
            public static ChildInfo translationConstraintChild;
            public static ChildInfo animclipChild;
            public static ChildInfo blendChild;
            public static ChildInfo blendshapeControllerChild;
            public static ChildInfo cgshaderChild;
            public static ChildInfo deformerChild;
            public static ChildInfo expressionChild;
            public static ChildInfo imageChild;
            public static ChildInfo materialChild;
            public static ChildInfo motionPathChild;
            public static ChildInfo objsetChild;
            public static ChildInfo pmdataATGChild;
            public static ChildInfo poseChild;
            public static ChildInfo sceneChild;
            public static ChildInfo shaderChild;
            public static ChildInfo textureChild;
            public static ChildInfo blendshapeChild;
            public static ChildInfo skinChild;
            public static ChildInfo customDataChild;
        }

        public static class shaderType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static ChildInfo bindingChild;
            public static ChildInfo animChannelChild;
            public static ChildInfo animDiscontinuitiesChild;
            public static ChildInfo animChild;
            public static ChildInfo aimConstraintChild;
            public static ChildInfo atgilocatorChild;
            public static ChildInfo blendtargetChild;
            public static ChildInfo cameraChild;
            public static ChildInfo constraintChild;
            public static ChildInfo clusterChild;
            public static ChildInfo dynamicTypeChild;
            public static ChildInfo instanceChild;
            public static ChildInfo jointChild;
            public static ChildInfo lightChild;
            public static ChildInfo locatorChild;
            public static ChildInfo lodgroupChild;
            public static ChildInfo meshChild;
            public static ChildInfo multiBlendTargetChild;
            public static ChildInfo nodeChild;
            public static ChildInfo nurbsCurveChild;
            public static ChildInfo nurbsChild;
            public static ChildInfo orientationConstraintChild;
            public static ChildInfo parentConstraintChild;
            public static ChildInfo primitiveChild;
            public static ChildInfo referenceChild;
            public static ChildInfo rigidBodyChild;
            public static ChildInfo scaleConstraintChild;
            public static ChildInfo springConstraintChild;
            public static ChildInfo translationConstraintChild;
            public static ChildInfo animclipChild;
            public static ChildInfo blendChild;
            public static ChildInfo blendshapeControllerChild;
            public static ChildInfo cgshaderChild;
            public static ChildInfo deformerChild;
            public static ChildInfo expressionChild;
            public static ChildInfo imageChild;
            public static ChildInfo materialChild;
            public static ChildInfo motionPathChild;
            public static ChildInfo objsetChild;
            public static ChildInfo pmdataATGChild;
            public static ChildInfo poseChild;
            public static ChildInfo sceneChild;
            public static ChildInfo shaderChild;
            public static ChildInfo textureChild;
            public static ChildInfo blendshapeChild;
            public static ChildInfo skinChild;
            public static ChildInfo customDataChild;
        }

        public static class shaderType_binding
        {
            public static DomNodeType Type;
            public static AttributeInfo Attribute;
            public static AttributeInfo tagAttribute;
            public static AttributeInfo typeAttribute;
            public static AttributeInfo sourceAttribute;
            public static AttributeInfo datasetAttribute;
            public static AttributeInfo countAttribute;
        }

        public static class textureType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo uriAttribute;
            public static ChildInfo animChannelChild;
            public static ChildInfo animDiscontinuitiesChild;
            public static ChildInfo animChild;
            public static ChildInfo aimConstraintChild;
            public static ChildInfo atgilocatorChild;
            public static ChildInfo blendtargetChild;
            public static ChildInfo cameraChild;
            public static ChildInfo constraintChild;
            public static ChildInfo clusterChild;
            public static ChildInfo dynamicTypeChild;
            public static ChildInfo instanceChild;
            public static ChildInfo jointChild;
            public static ChildInfo lightChild;
            public static ChildInfo locatorChild;
            public static ChildInfo lodgroupChild;
            public static ChildInfo meshChild;
            public static ChildInfo multiBlendTargetChild;
            public static ChildInfo nodeChild;
            public static ChildInfo nurbsCurveChild;
            public static ChildInfo nurbsChild;
            public static ChildInfo orientationConstraintChild;
            public static ChildInfo parentConstraintChild;
            public static ChildInfo primitiveChild;
            public static ChildInfo referenceChild;
            public static ChildInfo rigidBodyChild;
            public static ChildInfo scaleConstraintChild;
            public static ChildInfo springConstraintChild;
            public static ChildInfo translationConstraintChild;
            public static ChildInfo animclipChild;
            public static ChildInfo blendChild;
            public static ChildInfo blendshapeControllerChild;
            public static ChildInfo cgshaderChild;
            public static ChildInfo deformerChild;
            public static ChildInfo expressionChild;
            public static ChildInfo imageChild;
            public static ChildInfo materialChild;
            public static ChildInfo motionPathChild;
            public static ChildInfo objsetChild;
            public static ChildInfo pmdataATGChild;
            public static ChildInfo poseChild;
            public static ChildInfo sceneChild;
            public static ChildInfo shaderChild;
            public static ChildInfo textureChild;
            public static ChildInfo blendshapeChild;
            public static ChildInfo skinChild;
            public static ChildInfo customDataChild;
        }

        public static class blendshapeType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo controllerAttribute;
            public static ChildInfo targetChild;
            public static ChildInfo animChannelChild;
            public static ChildInfo animDiscontinuitiesChild;
            public static ChildInfo animChild;
            public static ChildInfo aimConstraintChild;
            public static ChildInfo atgilocatorChild;
            public static ChildInfo blendtargetChild;
            public static ChildInfo cameraChild;
            public static ChildInfo constraintChild;
            public static ChildInfo clusterChild;
            public static ChildInfo dynamicTypeChild;
            public static ChildInfo instanceChild;
            public static ChildInfo jointChild;
            public static ChildInfo lightChild;
            public static ChildInfo locatorChild;
            public static ChildInfo lodgroupChild;
            public static ChildInfo meshChild;
            public static ChildInfo multiBlendTargetChild;
            public static ChildInfo nodeChild;
            public static ChildInfo nurbsCurveChild;
            public static ChildInfo nurbsChild;
            public static ChildInfo orientationConstraintChild;
            public static ChildInfo parentConstraintChild;
            public static ChildInfo primitiveChild;
            public static ChildInfo referenceChild;
            public static ChildInfo rigidBodyChild;
            public static ChildInfo scaleConstraintChild;
            public static ChildInfo springConstraintChild;
            public static ChildInfo translationConstraintChild;
            public static ChildInfo animclipChild;
            public static ChildInfo blendChild;
            public static ChildInfo blendshapeControllerChild;
            public static ChildInfo cgshaderChild;
            public static ChildInfo deformerChild;
            public static ChildInfo expressionChild;
            public static ChildInfo imageChild;
            public static ChildInfo materialChild;
            public static ChildInfo motionPathChild;
            public static ChildInfo objsetChild;
            public static ChildInfo pmdataATGChild;
            public static ChildInfo poseChild;
            public static ChildInfo sceneChild;
            public static ChildInfo shaderChild;
            public static ChildInfo textureChild;
            public static ChildInfo blendshapeChild;
            public static ChildInfo skinChild;
            public static ChildInfo customDataChild;
        }

        public static class blendshapeType_target
        {
            public static DomNodeType Type;
            public static AttributeInfo weightIndexAttribute;
            public static ChildInfo animChannelChild;
            public static ChildInfo animDiscontinuitiesChild;
            public static ChildInfo animChild;
            public static ChildInfo aimConstraintChild;
            public static ChildInfo atgilocatorChild;
            public static ChildInfo blendtargetChild;
            public static ChildInfo cameraChild;
            public static ChildInfo constraintChild;
            public static ChildInfo clusterChild;
            public static ChildInfo dynamicTypeChild;
            public static ChildInfo instanceChild;
            public static ChildInfo jointChild;
            public static ChildInfo lightChild;
            public static ChildInfo locatorChild;
            public static ChildInfo lodgroupChild;
            public static ChildInfo meshChild;
            public static ChildInfo multiBlendTargetChild;
            public static ChildInfo nodeChild;
            public static ChildInfo nurbsCurveChild;
            public static ChildInfo nurbsChild;
            public static ChildInfo orientationConstraintChild;
            public static ChildInfo parentConstraintChild;
            public static ChildInfo primitiveChild;
            public static ChildInfo referenceChild;
            public static ChildInfo rigidBodyChild;
            public static ChildInfo scaleConstraintChild;
            public static ChildInfo springConstraintChild;
            public static ChildInfo translationConstraintChild;
            public static ChildInfo animclipChild;
            public static ChildInfo blendChild;
            public static ChildInfo blendshapeControllerChild;
            public static ChildInfo cgshaderChild;
            public static ChildInfo deformerChild;
            public static ChildInfo expressionChild;
            public static ChildInfo imageChild;
            public static ChildInfo materialChild;
            public static ChildInfo motionPathChild;
            public static ChildInfo objsetChild;
            public static ChildInfo pmdataATGChild;
            public static ChildInfo poseChild;
            public static ChildInfo sceneChild;
            public static ChildInfo shaderChild;
            public static ChildInfo textureChild;
            public static ChildInfo blendshapeChild;
            public static ChildInfo skinChild;
            public static ChildInfo customDataChild;
        }

        public static class skinType
        {
            public static DomNodeType Type;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo typeAttribute;
            public static ChildInfo influenceChild;
            public static ChildInfo weightsChild;
            public static ChildInfo componentsChild;
            public static ChildInfo animChannelChild;
            public static ChildInfo animDiscontinuitiesChild;
            public static ChildInfo animChild;
            public static ChildInfo aimConstraintChild;
            public static ChildInfo atgilocatorChild;
            public static ChildInfo blendtargetChild;
            public static ChildInfo cameraChild;
            public static ChildInfo constraintChild;
            public static ChildInfo clusterChild;
            public static ChildInfo dynamicTypeChild;
            public static ChildInfo instanceChild;
            public static ChildInfo jointChild;
            public static ChildInfo lightChild;
            public static ChildInfo locatorChild;
            public static ChildInfo lodgroupChild;
            public static ChildInfo meshChild;
            public static ChildInfo multiBlendTargetChild;
            public static ChildInfo nodeChild;
            public static ChildInfo nurbsCurveChild;
            public static ChildInfo nurbsChild;
            public static ChildInfo orientationConstraintChild;
            public static ChildInfo parentConstraintChild;
            public static ChildInfo primitiveChild;
            public static ChildInfo referenceChild;
            public static ChildInfo rigidBodyChild;
            public static ChildInfo scaleConstraintChild;
            public static ChildInfo springConstraintChild;
            public static ChildInfo translationConstraintChild;
            public static ChildInfo animclipChild;
            public static ChildInfo blendChild;
            public static ChildInfo blendshapeControllerChild;
            public static ChildInfo cgshaderChild;
            public static ChildInfo deformerChild;
            public static ChildInfo expressionChild;
            public static ChildInfo imageChild;
            public static ChildInfo materialChild;
            public static ChildInfo motionPathChild;
            public static ChildInfo objsetChild;
            public static ChildInfo pmdataATGChild;
            public static ChildInfo poseChild;
            public static ChildInfo sceneChild;
            public static ChildInfo shaderChild;
            public static ChildInfo textureChild;
            public static ChildInfo blendshapeChild;
            public static ChildInfo skinChild;
            public static ChildInfo customDataChild;
        }

        public static class skinType_influence
        {
            public static DomNodeType Type;
            public static AttributeInfo bindInverseAttribute;
            public static AttributeInfo targetAttribute;
        }

        public static class skinType_weights
        {
            public static DomNodeType Type;
            public static AttributeInfo Attribute;
            public static AttributeInfo countAttribute;
        }

        public static class skinType_components
        {
            public static DomNodeType Type;
            public static AttributeInfo Attribute;
            public static AttributeInfo countAttribute;
        }

        public static class customDataType
        {
            public static DomNodeType Type;
            public static ChildInfo attributeChild;
        }

        public static class customDataAttributeType
        {
            public static DomNodeType Type;
            public static AttributeInfo fieldAttribute;
            public static AttributeInfo Attribute;
            public static AttributeInfo nameAttribute;
            public static AttributeInfo typeAttribute;
            public static AttributeInfo valueAttribute;
            public static AttributeInfo defaultAttribute;
            public static AttributeInfo minAttribute;
            public static AttributeInfo maxAttribute;
            public static AttributeInfo countAttribute;
            public static AttributeInfo indexAttribute;
            public static AttributeInfo isArrayAttribute;
            public static ChildInfo valueChild;
        }

        public static class jointType_freedoms
        {
            public static DomNodeType Type;
            public static AttributeInfo channelsAttribute;
        }

        public static class jointType_minrotation
        {
            public static DomNodeType Type;
            public static AttributeInfo Attribute;
            public static AttributeInfo channelsAttribute;
        }

        public static class jointType_maxrotation
        {
            public static DomNodeType Type;
            public static AttributeInfo Attribute;
            public static AttributeInfo channelsAttribute;
        }

        public static class jointType_jointOrientEul
        {
            public static DomNodeType Type;
            public static AttributeInfo Attribute;
            public static AttributeInfo rotOrdAttribute;
        }

        public static ChildInfo ATGRootElement;
    }
}
