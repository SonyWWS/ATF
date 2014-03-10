//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using NUnit.Framework;

using Sce.Atf.Applications;

namespace UnitTests.Atf
{
    [TestFixture]
    public class TestRenameCommand
    {
        /// <summary>
        /// Creates a new name by parsing 'original' and replacing pieces or adding to it.</summary>
        /// <param name="original">The original name. Can't be null.</param>
        /// <param name="prefix">Optional prefix that is placed at the beginning of the result. Can be null.
        /// If 'original' already has the prefix, it won't be added a 2nd time.</param>
        /// <param name="baseName">Optional base name that is concatenated with the prefix. If null, then
        /// the corresponding part of 'original' is used, otherwise 'original' is ignored.</param>
        /// <param name="suffix">Optional suffix that is placed after the base name. Can be null.</param>
        /// <param name="numericSuffix">Optional positive numeric suffix that is placed after the suffix.
        /// If negative, then this parameter is ignored. If non-negative, then any existing numeric
        /// suffix on 'original' will be removed.</param>
        /// <returns>The new name</returns>
        [TestCase("Original", null, null, null, -1, Result = "Original")]
        [TestCase("Original", "pre_", null, null, -1, Result = "pre_Original")]
        [TestCase("pre_Original", "pre_", null, null, -1, Result = "pre_Original")]
        [TestCase("Original", null, null, "_s", -1, Result = "Original_s")]
        [TestCase("Original_s", null, null, "_s", -1, Result = "Original_s")]
        [TestCase("Original", null, null, "32", -1, Result = "Original32")]
        [TestCase("Original", null, null, null, 32, Result = "Original32")]
        [TestCase("Original", "pre_", null, "_", 32, Result = "pre_Original_32")]
        [TestCase("Original", "pre_", "NewBase", "_", 32, Result = "pre_NewBase_32")]
        [TestCase("Original_64", "pre_", "NewBase", "_", 32, Result = "pre_NewBase_32")]
        [TestCase("pre_Original_64", "pre_", "NewBase", "_", 32, Result = "pre_NewBase_32")]
        [TestCase("pre_Original_64", "pre_", null, "_", 32, Result = "pre_Original_32")]
        [TestCase("Original64", "pre_", null, "_", 1234567890, Result = "pre_Original_1234567890")]
        [TestCase("Original1234567890", null, null, null, 1, Result = "Original1")]
        public string TestRename(string original, string prefix, string baseName, string suffix, long numericSuffix = -1)
        {
            return RenameCommand.Rename(original, prefix, baseName, suffix, numericSuffix);
        }
    }
}
