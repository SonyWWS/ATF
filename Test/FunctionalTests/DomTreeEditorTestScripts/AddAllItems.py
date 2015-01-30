#Copyright (c) 2014 Sony Computer Entertainment America LLC. See License.txt.

import sys
sys.path.append("./CommonTestScripts")
import Test

doc = atfDocService.OpenNewDocument(editor)

#=====================  0: root ==================================
Test.Equal(0, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(treeLister.TreeView.DomNode)), "Verify root child count")
package = editingContext.Insert[UIPackage](DomNode(UISchema.UIPackageType.Type), treeLister.TreeView.DomNode)
Test.Equal(1, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(treeLister.TreeView.DomNode)), "Verify root child count")

print "Trying to add objects that cannot be a child of the root"
editingContext.Insert[UIForm](DomNode(UISchema.UIFormType.Type), treeLister.TreeView.DomNode)
Test.Equal(1, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(treeLister.TreeView.DomNode)), "Verify root child count does not increase when adding a form")
editingContext.Insert[UIShader](DomNode(UISchema.UIShaderType.Type), treeLister.TreeView.DomNode)
Test.Equal(1, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(treeLister.TreeView.DomNode)), "Verify root child count does not increase when adding a shader")
editingContext.Insert[UITexture](DomNode(UISchema.UITextureType.Type), treeLister.TreeView.DomNode)
Test.Equal(1, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(treeLister.TreeView.DomNode)), "Verify root child count does not increase when adding a texture")
editingContext.Insert[UIFont](DomNode(UISchema.UIFontType.Type), treeLister.TreeView.DomNode)
Test.Equal(1, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(treeLister.TreeView.DomNode)), "Verify root child count does not increase when adding a font")
editingContext.Insert[UISprite](DomNode(UISchema.UISpriteType.Type), treeLister.TreeView.DomNode)
Test.Equal(1, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(treeLister.TreeView.DomNode)), "Verify root child count does not increase when adding a sprite")
editingContext.Insert[UITextItem](DomNode(UISchema.UITextItemType.Type), treeLister.TreeView.DomNode)
Test.Equal(1, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(treeLister.TreeView.DomNode)), "Verify root child count does not increase when adding a text")
editingContext.Insert[UIAnimation](DomNode(UISchema.UIAnimationType.Type), treeLister.TreeView.DomNode)
Test.Equal(1, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(treeLister.TreeView.DomNode)), "Verify root child count does not increase when adding an animation")

#=====================  1: Package ==================================
print "Adding children to a package"
Test.Equal(0, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(package.DomNode)), "Verify package child count")
form = editingContext.Insert[UIForm](DomNode(UISchema.UIFormType.Type), package.DomNode)
Test.Equal(1, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(package.DomNode)), "Verify package child count after adding form")
shader = editingContext.Insert[UIShader](DomNode(UISchema.UIShaderType.Type), package.DomNode)
Test.Equal(2, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(package.DomNode)), "Verify package child count after adding shader")
texture = editingContext.Insert[UITexture](DomNode(UISchema.UITextureType.Type), package.DomNode)
Test.Equal(3, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(package.DomNode)), "Verify package child count after adding texture")
font = editingContext.Insert[UIFont](DomNode(UISchema.UIFontType.Type), package.DomNode)
Test.Equal(4, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(package.DomNode)), "Verify package child count after adding font")
packageChildCount = 4

print "Trying to add objects that cannot be a child of a package"
editingContext.Insert[UIPackage](DomNode(UISchema.UIPackageType.Type), package.DomNode)
Test.Equal(packageChildCount, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(package.DomNode)), "Verify package child count does not increase after adding package")
editingContext.Insert[UISprite](DomNode(UISchema.UISpriteType.Type), package.DomNode)
Test.Equal(packageChildCount, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(package.DomNode)), "Verify package child count does not increase after adding sprite")
editingContext.Insert[UITextItem](DomNode(UISchema.UITextItemType.Type), package.DomNode)
Test.Equal(packageChildCount, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(package.DomNode)), "Verify package child count does not increase after adding text")
editingContext.Insert[UIAnimation](DomNode(UISchema.UIAnimationType.Type), package.DomNode)
Test.Equal(packageChildCount, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(package.DomNode)), "Verify package child count does not increase after adding animation")

#=====================  2: Form ==================================
print "Adding children to a form"
Test.Equal(0, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(form.DomNode)), "Verify form child count")
sprite = editingContext.Insert[UISprite](DomNode(UISchema.UISpriteType.Type), form.DomNode)
Test.Equal(1, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(form.DomNode)), "Verify form child count after adding sprite")
text = editingContext.Insert[UITextItem](DomNode(UISchema.UITextItemType.Type), form.DomNode)
Test.Equal(2, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(form.DomNode)), "Verify form child count after adding text")
animation = editingContext.Insert[UIAnimation](DomNode(UISchema.UIAnimationType.Type), form.DomNode)
Test.Equal(3, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(form.DomNode)), "Verify form child count after adding animation")

print "Trying to add objects that cannot be a child of a form"
editingContext.Insert[UIPackage](DomNode(UISchema.UIPackageType.Type), form.DomNode)
Test.Equal(3, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(form.DomNode)), "Verify form child count does not increase after adding a package")
editingContext.Insert[UIForm](DomNode(UISchema.UIFormType.Type), form.DomNode)
Test.Equal(3, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(form.DomNode)), "Verify form child count does not increase after adding a form")
editingContext.Insert[UIShader](DomNode(UISchema.UIShaderType.Type), form.DomNode)
Test.Equal(3, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(form.DomNode)), "Verify form child count does not increase after adding a shader")
editingContext.Insert[UITexture](DomNode(UISchema.UITextureType.Type), form.DomNode)
Test.Equal(3, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(form.DomNode)), "Verify form child count does not increase after adding a texture")
editingContext.Insert[UIFont](DomNode(UISchema.UIFontType.Type), form.DomNode)
Test.Equal(3, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(form.DomNode)), "Verify form child count does not increase after adding a font")

#=====================  3: Shader ==================================
print "Verify cannot add children to a shader"
editingContext.Insert[UIPackage](DomNode(UISchema.UIPackageType.Type), shader.DomNode)
Test.Equal(0, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(shader.DomNode)), "Verify shader child count does not increase when adding a package")
editingContext.Insert[UIForm](DomNode(UISchema.UIFormType.Type), shader.DomNode)
Test.Equal(0, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(shader.DomNode)), "Verify shader child count does not increase when adding a form")
editingContext.Insert[UIShader](DomNode(UISchema.UIShaderType.Type), shader.DomNode)
Test.Equal(0, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(shader.DomNode)), "Verify shader child count does not increase when adding a shader")
editingContext.Insert[UITexture](DomNode(UISchema.UITextureType.Type), shader.DomNode)
Test.Equal(0, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(shader.DomNode)), "Verify shader child count does not increase when adding a texture")
editingContext.Insert[UIFont](DomNode(UISchema.UIFontType.Type), shader.DomNode)
Test.Equal(0, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(shader.DomNode)), "Verify shader child count does not increase when adding a font")
editingContext.Insert[UISprite](DomNode(UISchema.UISpriteType.Type), shader.DomNode)
Test.Equal(0, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(shader.DomNode)), "Verify shader child count does not increase when adding a sprite")
editingContext.Insert[UITextItem](DomNode(UISchema.UITextItemType.Type), shader.DomNode)
Test.Equal(0, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(shader.DomNode)), "Verify shader child count does not increase when adding a text")
editingContext.Insert[UIAnimation](DomNode(UISchema.UIAnimationType.Type), shader.DomNode)
Test.Equal(0, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(shader.DomNode)), "Verify shader child count does not increase when adding an animation")

#=====================  4: Texture ==================================
print "Verify cannot add children to a texture"
editingContext.Insert[UIPackage](DomNode(UISchema.UIPackageType.Type), texture.DomNode)
Test.Equal(0, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(texture.DomNode)), "Verify texture child count does not increase when adding a package")
editingContext.Insert[UIForm](DomNode(UISchema.UIFormType.Type), texture.DomNode)
Test.Equal(0, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(texture.DomNode)), "Verify texture child count does not increase when adding a form")
editingContext.Insert[UIShader](DomNode(UISchema.UIShaderType.Type), texture.DomNode)
Test.Equal(0, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(texture.DomNode)), "Verify texture child count does not increase when adding a shader")
editingContext.Insert[UITexture](DomNode(UISchema.UITextureType.Type), texture.DomNode)
Test.Equal(0, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(texture.DomNode)), "Verify texture child count does not increase when adding a texture")
editingContext.Insert[UIFont](DomNode(UISchema.UIFontType.Type), texture.DomNode)
Test.Equal(0, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(texture.DomNode)), "Verify texture child count does not increase when adding a font")
editingContext.Insert[UISprite](DomNode(UISchema.UISpriteType.Type), texture.DomNode)
Test.Equal(0, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(texture.DomNode)), "Verify texture child count does not increase when adding a sprite")
editingContext.Insert[UITextItem](DomNode(UISchema.UITextItemType.Type), texture.DomNode)
Test.Equal(0, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(texture.DomNode)), "Verify texture child count does not increase when adding a text")
editingContext.Insert[UIAnimation](DomNode(UISchema.UIAnimationType.Type), texture.DomNode)
Test.Equal(0, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(texture.DomNode)), "Verify texture child count does not increase when adding an animation")

#=====================  5: Font ==================================
print "Verify cannot add children to a font"
editingContext.Insert[UIPackage](DomNode(UISchema.UIPackageType.Type), font.DomNode)
Test.Equal(0, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(font.DomNode)), "Verify font child count does not increase when adding a package")
editingContext.Insert[UIForm](DomNode(UISchema.UIFormType.Type), font.DomNode)
Test.Equal(0, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(font.DomNode)), "Verify font child count does not increase when adding a form")
editingContext.Insert[UIShader](DomNode(UISchema.UIShaderType.Type), font.DomNode)
Test.Equal(0, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(font.DomNode)), "Verify font child count does not increase when adding a shader")
editingContext.Insert[UITexture](DomNode(UISchema.UITextureType.Type), font.DomNode)
Test.Equal(0, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(font.DomNode)), "Verify font child count does not increase when adding a texture")
editingContext.Insert[UIFont](DomNode(UISchema.UIFontType.Type), font.DomNode)
Test.Equal(0, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(font.DomNode)), "Verify font child count does not increase when adding a font")
editingContext.Insert[UISprite](DomNode(UISchema.UISpriteType.Type), font.DomNode)
Test.Equal(0, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(font.DomNode)), "Verify font child count does not increase when adding a sprite")
editingContext.Insert[UITextItem](DomNode(UISchema.UITextItemType.Type), font.DomNode)
Test.Equal(0, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(font.DomNode)), "Verify font child count does not increase when adding a text")
editingContext.Insert[UIAnimation](DomNode(UISchema.UIAnimationType.Type), font.DomNode)
Test.Equal(0, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(font.DomNode)), "Verify font child count does not increase when adding an animation")

#=====================  6: Sprite ==================================
print "Adding children to a sprite"
Test.Equal(2, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(sprite.DomNode)), "Verify sprite child count (starts with a transform and an empty ref)")
spriteUnderSprite = editingContext.Insert[UISprite](DomNode(UISchema.UISpriteType.Type), sprite.DomNode)
Test.Equal(3, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(sprite.DomNode)), "Verify sprite child count after adding sprite")
textUnderSprite = editingContext.Insert[UITextItem](DomNode(UISchema.UITextItemType.Type), sprite.DomNode)
Test.Equal(4, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(sprite.DomNode)), "Verify sprite child count after adding text")
animationUnderSprite = editingContext.Insert[UIAnimation](DomNode(UISchema.UIAnimationType.Type), sprite.DomNode)
Test.Equal(5, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(sprite.DomNode)), "Verify sprite child count after adding animation")
#must be added as ref:
shaderUnderSprite = editingContext.InsertAsRef[UIShader](DomNode(UISchema.UIShaderType.Type), sprite.DomNode)
Test.Equal(5, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(sprite.DomNode)), "Verify sprite child count after adding shader")
#refs will be added as real objects to the package
packageChildCount = packageChildCount + 1
Test.Equal(packageChildCount, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(package.DomNode)), "Verify package child count increases after adding a ref")

print "Trying to add objects that cannot be a child of a sprite"
editingContext.Insert[UIPackage](DomNode(UISchema.UIPackageType.Type), sprite.DomNode)
Test.Equal(5, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(sprite.DomNode)), "Verify sprite child count does not increase when adding a package")
editingContext.Insert[UIForm](DomNode(UISchema.UIFormType.Type), sprite.DomNode)
Test.Equal(5, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(sprite.DomNode)), "Verify sprite child count does not increase when adding a form")
editingContext.Insert[UITexture](DomNode(UISchema.UITextureType.Type), sprite.DomNode)
Test.Equal(5, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(sprite.DomNode)), "Verify sprite child count does not increase when adding a texture")
editingContext.Insert[UIFont](DomNode(UISchema.UIFontType.Type), sprite.DomNode)
Test.Equal(5, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(sprite.DomNode)), "Verify sprite child count does not increase when adding a font")
editingContext.Insert[UIShader](DomNode(UISchema.UIShaderType.Type), sprite.DomNode)
Test.Equal(5, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(sprite.DomNode)), "Verify sprite child count does not increase when adding a shader")

#=====================  7: Text ==================================
print "Adding children to a text"
Test.Equal(2, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(text.DomNode)), "Verify text child count (starts with a transform and an empty ref)")
spriteUnderText = editingContext.Insert[UISprite](DomNode(UISchema.UISpriteType.Type), text.DomNode)
Test.Equal(3, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(text.DomNode)), "Verify text child count after adding sprite")
textUnderText = editingContext.Insert[UITextItem](DomNode(UISchema.UITextItemType.Type), text.DomNode)
Test.Equal(4, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(text.DomNode)), "Verify text child count after adding text")
animationUnderText = editingContext.Insert[UIAnimation](DomNode(UISchema.UIAnimationType.Type), text.DomNode)
Test.Equal(5, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(text.DomNode)), "Verify text child count after adding animation")
#must be added as ref:
fontUnderText = editingContext.InsertAsRef[UIFont](DomNode(UISchema.UIFontType.Type), text.DomNode)
Test.Equal(5, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(text.DomNode)), "Verify text child count after adding font as ref")
packageChildCount = packageChildCount + 1
Test.Equal(packageChildCount, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(package.DomNode)), "Verify package child count increases after adding a ref")

print "Trying to add objects that cannot be a child of a text"
editingContext.Insert[UIPackage](DomNode(UISchema.UIPackageType.Type), text.DomNode)
Test.Equal(5, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(text.DomNode)), "Verify text child count does not increase when adding a package")
editingContext.Insert[UIForm](DomNode(UISchema.UIFormType.Type), text.DomNode)
Test.Equal(5, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(text.DomNode)), "Verify text child count does not increase when adding a form")
editingContext.Insert[UIShader](DomNode(UISchema.UIShaderType.Type), text.DomNode)
Test.Equal(5, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(text.DomNode)), "Verify text child count does not increase when adding a shader")
editingContext.Insert[UITexture](DomNode(UISchema.UITextureType.Type), text.DomNode)
Test.Equal(5, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(text.DomNode)), "Verify text child count does not increase when adding a texture")
editingContext.Insert[UIFont](DomNode(UISchema.UIFontType.Type), text.DomNode)
Test.Equal(5, Test.GetEnumerableCount(treeLister.TreeView.GetChildren(text.DomNode)), "Verify text child count does not increase when adding a font")

#=====================  8: Animation ==================================
print "Verify cannot add children to an animation"
animCount = Test.GetEnumerableCount(treeLister.TreeView.GetChildren(animation.DomNode))
editingContext.Insert[UIPackage](DomNode(UISchema.UIPackageType.Type), animation.DomNode)
Test.Equal(Test.GetEnumerableCount(treeLister.TreeView.GetChildren(animation.DomNode)), animCount, "Verify animation child count does not increase when adding a package")
editingContext.Insert[UIForm](DomNode(UISchema.UIFormType.Type), animation.DomNode)
Test.Equal(Test.GetEnumerableCount(treeLister.TreeView.GetChildren(animation.DomNode)), animCount, "Verify animation child count does not increase when adding a form")
editingContext.Insert[UIShader](DomNode(UISchema.UIShaderType.Type), animation.DomNode)
Test.Equal(Test.GetEnumerableCount(treeLister.TreeView.GetChildren(animation.DomNode)), animCount, "Verify animation child count does not increase when adding a shader")
editingContext.Insert[UITexture](DomNode(UISchema.UITextureType.Type), animation.DomNode)
Test.Equal(Test.GetEnumerableCount(treeLister.TreeView.GetChildren(animation.DomNode)), animCount, "Verify animation child count does not increase when adding a texture")
editingContext.Insert[UIFont](DomNode(UISchema.UIFontType.Type), animation.DomNode)
Test.Equal(Test.GetEnumerableCount(treeLister.TreeView.GetChildren(animation.DomNode)), animCount, "Verify animation child count does not increase when adding a font")
editingContext.Insert[UISprite](DomNode(UISchema.UISpriteType.Type), animation.DomNode)
Test.Equal(Test.GetEnumerableCount(treeLister.TreeView.GetChildren(animation.DomNode)), animCount, "Verify animation child count does not increase when adding a sprite")
editingContext.Insert[UITextItem](DomNode(UISchema.UITextItemType.Type), animation.DomNode)
Test.Equal(Test.GetEnumerableCount(treeLister.TreeView.GetChildren(animation.DomNode)), animCount, "Verify animation child count does not increase when adding a text")
editingContext.Insert[UIAnimation](DomNode(UISchema.UIAnimationType.Type), animation.DomNode)
Test.Equal(Test.GetEnumerableCount(treeLister.TreeView.GetChildren(animation.DomNode)), animCount, "Verify animation child count does not increase when adding an animation")

print Test.SUCCESS
