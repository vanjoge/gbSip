import { request } from '@/utils/request';
import Api from '@/core/permission/modules/superior';

export function getSuperiorList(params: API.PageParams<API.TSuperior>) {
  return request<API.TableListResult<API.TSuperiorList>>({
    url: Api.list,
    method: 'get',
    params,
  });
}
export function getChannelList(params: API.PageParams<API.TSuperiorChannel>) {
  return request<API.TableListResult<API.TSuperiorChannelList>>({
    url: Api.channelList,
    method: 'get',
    params,
  });
}
export function bindChannel(
  superiorId: string,
  add: API.TSuperiorChannel[],
  remove: API.TSuperiorChannel[],
) {
  return request(
    {
      url: Api.bindChannels,
      method: 'post',
      data: { SuperiorId: superiorId, Add: add, Remove: remove },
    },
    {
      successMsg: '绑定成功',
    },
  );
}
export function createSuperior(data: API.TSuperior) {
  return request(
    {
      url: Api.create,
      method: 'post',
      data,
    },
    {
      successMsg: '添加上级成功',
    },
  );
}

export function updateSuperior(data: API.TSuperior) {
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

export function deleteSuperior(data: { SuperiorIds: string[] }) {
  return request({
    url: Api.delete,
    method: 'post',
    data,
  });
}
