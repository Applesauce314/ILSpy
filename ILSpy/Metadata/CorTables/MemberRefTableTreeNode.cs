﻿// Copyright (c) 2011 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;

using ICSharpCode.Decompiler.Metadata;

namespace ICSharpCode.ILSpy.Metadata
{
	internal class MemberRefTableTreeNode : MetadataTableTreeNode
	{
		public MemberRefTableTreeNode(MetadataFile metadataFile)
			: base(TableIndex.MemberRef, metadataFile)
		{
		}

		public override bool View(ViewModels.TabPageModel tabPage)
		{
			tabPage.Title = Text.ToString();
			tabPage.SupportsLanguageSwitching = false;

			var view = Helpers.PrepareDataGrid(tabPage, this);
			var metadata = metadataFile.Metadata;

			var list = new List<MemberRefEntry>();
			MemberRefEntry scrollTargetEntry = default;

			foreach (var row in metadata.MemberReferences)
			{
				MemberRefEntry entry = new MemberRefEntry(metadataFile, row);
				if (entry.RID == this.scrollTarget)
				{
					scrollTargetEntry = entry;
				}
				list.Add(entry);
			}

			view.ItemsSource = list;

			tabPage.Content = view;

			if (scrollTargetEntry.RID > 0)
			{
				ScrollItemIntoView(view, scrollTargetEntry);
			}

			return true;
		}

		struct MemberRefEntry
		{
			readonly MetadataFile metadataFile;
			readonly MemberReferenceHandle handle;
			readonly MemberReference memberRef;

			public int RID => MetadataTokens.GetRowNumber(handle);

			public int Token => MetadataTokens.GetToken(handle);

			public int Offset => metadataFile.MetadataOffset
				+ metadataFile.Metadata.GetTableMetadataOffset(TableIndex.MemberRef)
				+ metadataFile.Metadata.GetTableRowSize(TableIndex.MemberRef) * (RID - 1);

			[ColumnInfo("X8", Kind = ColumnKind.Token)]
			public int Parent => MetadataTokens.GetToken(memberRef.Parent);

			public void OnParentClick()
			{
				MainWindow.Instance.JumpToReference(new EntityReference(metadataFile, memberRef.Parent, protocol: "metadata"));
			}

			string? parentTooltip;
			public string? ParentTooltip => GenerateTooltip(ref parentTooltip, metadataFile, memberRef.Parent);

			public string Name => metadataFile.Metadata.GetString(memberRef.Name);

			public string NameTooltip => $"{MetadataTokens.GetHeapOffset(memberRef.Name):X} \"{Name}\"";

			[ColumnInfo("X8", Kind = ColumnKind.HeapOffset)]
			public int Signature => MetadataTokens.GetHeapOffset(memberRef.Signature);

			string? signatureTooltip;
			public string? SignatureTooltip => GenerateTooltip(ref signatureTooltip, metadataFile, handle);

			public MemberRefEntry(MetadataFile metadataFile, MemberReferenceHandle handle)
			{
				this.metadataFile = metadataFile;
				this.handle = handle;
				this.memberRef = metadataFile.Metadata.GetMemberReference(handle);
				this.signatureTooltip = null;
				this.parentTooltip = null;
			}
		}
	}
}
