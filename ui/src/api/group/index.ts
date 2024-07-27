import { request } from '@/utils/request';
import Api from '@/core/permission/modules/group';

export function getGroups() {
  return request<API.TGroupList>({
    url: Api.list,
    method: 'get',
  });
}
export async function getAllGroupTreeTableData() {
  const data = await getGroups();
  return buildTree(data);
}
function buildTree(data: API.TGroupList) {
  const idMap = {};
  const tree: API.TGroupList = [];

  data.forEach((item) => {
    idMap[item.GroupId] = { ...item };
  });

  data.forEach((item) => {
    if (item.ParentId === null || item.ParentId == '') {
      tree.push(idMap[item.GroupId]);
    } else {
      const parent = idMap[item.ParentId];
      if (parent) {
        if (!parent.children) {
          parent.children = [];
        }
        parent.children.push(idMap[item.GroupId]);
      } else {
        tree.push(idMap[item.GroupId]);
      }
    }
  });

  return tree;
}
export function getGroupsByParent(params: { ParentId: string }) {
  return request<API.TableListResult<API.TGroupList>>({
    url: Api.getGroups,
    method: 'get',
    params,
  });
}
export function getChannelList(params: API.PageParams<API.TGroupChannel>) {
  return request<API.TableListResult<API.TGroupChannelList>>({
    url: Api.channelList,
    method: 'get',
    params,
  });
}
export function bindChannel(
  groupId: string,
  add: API.TGroupChannel[],
  remove: API.TGroupChannel[],
) {
  return request(
    {
      url: Api.bindChannels,
      method: 'post',
      data: { GroupId: groupId, Add: add, Remove: remove },
    },
    {
      successMsg: '绑定成功',
    },
  );
}
export function bindSuperior(data: API.TBindSuperior) {
  return request(
    {
      url: Api.bindSuperior,
      method: 'post',
      data,
    },
    {
      successMsg: '操作成功',
    },
  );
}
export function createGroup(data: API.TGroup) {
  return request(
    {
      url: Api.create,
      method: 'post',
      data,
    },
    {
      successMsg: '添加分组成功',
    },
  );
}

export function updateGroup(data: API.TGroup) {
  return request(
    {
      url: Api.update,
      method: 'post',
      data,
    },
    {
      successMsg: '修改上级成功',
    },
  );
}

export function deleteGroup(data: { Ids: string[] }) {
  return request({
    url: Api.delete,
    method: 'post',
    data,
  });
}
