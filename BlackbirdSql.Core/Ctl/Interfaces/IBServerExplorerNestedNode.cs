// Warning: Some assembly references could not be resolved automatically. This might lead to incorrect decompilation of some parts,
// for ex. property getter/setter access. To get optimal decompilation results, please manually add the missing references to the list of loaded assemblies.
// Microsoft.VisualStudio.ServerExplorer, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VSDesigner.ServerExplorer.INestedHierarchyNodeSite
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VSDesigner.ServerExplorer;

public interface INestedHierarchyNodeSite : INodeSite
{
	IVsHierarchy GetVsHierarchy();

	int GetVsItemId();
}
