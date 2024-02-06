using HSMCommon.Collections;
using HSMServer.Extensions;
using HSMServer.Model.Authentication;
using HSMServer.Model.Folders;
using HSMServer.Model.TreeViewModel;
using HSMServer.Model.UserTreeShallowCopy;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace HSMServer.Model.TreeViewModels;

public class RemoveNodesRequestModel
{
    public Guid[] NodeIds { get; set; }
}


public sealed class VisibleTreeViewModel
{
    private readonly ConcurrentDictionary<Guid, NodeShallowModel> _allTree = new();
    private readonly CHash<Guid> _openedNodes = new();

    private readonly User _user;

    public event Func<User, List<ProductNodeViewModel>> GetUserProducts;
    public event Func<List<FolderModel>> GetFolders;


    internal SensorRenderedHash SearchedSensors { get; } = new();


    public VisibleTreeViewModel(User user)
    {
        _user = user;
    }


    public void AddOpenedNode(Guid id) => _openedNodes.Add(id);

    public void RemoveOpenedNode(params Guid[] ids) => _openedNodes.Remove(ids);

    public void ClearOpenedNodes() => _openedNodes.Clear();


    public List<BaseShallowModel> GetUserTree(string searchParameter = null)
    {
        // products should be updated before folders because folders should contain updated products
        var products = GetUserProducts?.Invoke(_user).GetOrdered(_user);
        var folders = GetFolders?.Invoke().GetOrdered(_user).ToDictionary(k => k.Id, v => new FolderShallowModel(v, _user));

        var folderTree = new List<BaseShallowModel>(1 << 4);
        var tree = new List<BaseShallowModel>(1 << 4);
        var isSearchTree = !string.IsNullOrEmpty(searchParameter);

        SearchedSensors.Clear();
        _allTree.Clear();

        if (isSearchTree)
            ClearOpenedNodes();

        foreach (var product in products)
        {
            var shouldAddNode = isSearchTree ? CanAddNodeByName(product, searchParameter, out var node)
                                             : CanAddNode(product, out node);

            if (shouldAddNode)
            {
                var folderId = node.Data.FolderId;

                if (folderId.HasValue && folders.TryGetValue(folderId.Value, out var folder))
                    folder.AddChild(node, _user);
                else
                    tree.Add(node);
            }
        }

        Func<FolderShallowModel, bool> filter = isSearchTree ? folder =>
        {
            var isFolderEmpty = folder.IsEmpty;

            if (folder.Products.Count == 1)
            {
                if (isFolderEmpty && folder.Products[0].ContentIsEmpty)
                    isFolderEmpty = !folder.Products[0].IsNameContainsPattern(searchParameter);
            }

            return !isFolderEmpty || (folder.IsNameContainsPattern(searchParameter) && IsVisibleFolderForUser(folder.Id));
        }
        : folder => !folder.IsEmpty || IsVisibleFolderForUser(folder.Id);

        folderTree.AddRange(folders.Values.Where(filter));
        folderTree.AddRange(tree);

        return folderTree;
    }

    public NodeShallowModel LoadNode(ProductNodeViewModel globalModel)
    {
        var id = globalModel.Id;

        if (_allTree.TryGetValue(id, out var node) && IsVisibleNodeForUser(node))
        {
            node.LoadRenderingNodes();
            AddOpenedNode(id);
        }

        return node;
    }

    private NodeShallowModel FilterNodes(ProductNodeViewModel product, string searchParameter, out bool toRender)
    {
        var node = new NodeShallowModel(product, _user, IsVisibleNodeForUser, IsVisibleSensorForUser);

        toRender = false;
        _allTree.TryAdd(product.Id, node);

        foreach (var nodeModel in GetSubNodes(product))
        {
            var subNode = node.AddChild(FilterNodes(nodeModel, searchParameter, out var currentNodeToRender));

            if (subNode.IsNameContainsPattern(searchParameter) || currentNodeToRender)
            {
                toRender = node.ToRenderNode(subNode.Id);
                AddOpenedNode(subNode.Id);
            }
        }

        foreach (var sensorModel in GetSubSensors(product))
        {
            var sensor = node.AddChild(new SensorShallowModel(sensorModel, _user), _user);

            if (sensor.IsNameContainsPattern(searchParameter))
            {
                toRender = node.ToRenderNode(sensor.Id);

                if (toRender)
                    SearchedSensors.Add(sensor.Id);
            }
        }

        return node;
    }

    private NodeShallowModel FilterNodes(ProductNodeViewModel product, int depth = 1)
    {
        var node = new NodeShallowModel(product, _user, IsVisibleNodeForUser, IsVisibleSensorForUser);

        _allTree.TryAdd(product.Id, node);

        var toRender = _openedNodes.Contains(product.Id) || depth > 0;

        foreach (var nodeModel in GetSubNodes(product))
        {
            var subNode = node.AddChild(FilterNodes(nodeModel, --depth));

            if (toRender)
                node.ToRenderNode(subNode.Id);
        }

        foreach (var sensorModel in GetSubSensors(product))
        {
            var sensor = node.AddChild(new SensorShallowModel(sensorModel, _user), _user);

            if (toRender)
                node.ToRenderNode(sensor.Id);
        }

        return node;
    }

    private bool CanAddNode(ProductNodeViewModel product, out NodeShallowModel node)
    {
        node = FilterNodes(product);

        return IsVisibleNodeForUser(node);
    }

    private bool CanAddNodeByName(ProductNodeViewModel product, string searchParameter, out NodeShallowModel node)
    {
        node = FilterNodes(product, searchParameter, out var toRender);

        bool isVisible = IsVisibleNodeForUser(node) && toRender || node.IsNameContainsPattern(searchParameter);

        if (isVisible)
            AddOpenedNode(node.Id);

        return isVisible;
    }

    private IOrderedEnumerable<SensorNodeViewModel> GetSubSensors(ProductNodeViewModel product) => product.Sensors.Values.GetOrdered(_user);

    private IOrderedEnumerable<ProductNodeViewModel> GetSubNodes(ProductNodeViewModel product) => product.Nodes.Values.GetOrdered(_user);


    private bool IsVisibleNodeForUser(NodeShallowModel node) => node.VisibleSubtreeSensorsCount > 0 || _user.IsEmptyProductVisible(node.Data);

    private bool IsVisibleFolderForUser(Guid folderId) => _user.IsFolderAvailable(folderId) && _user.TreeFilter.ByVisibility.Empty.Value;

    private bool IsVisibleSensorForUser(SensorShallowModel sensor) => _user.IsSensorVisible(sensor.Data);
}