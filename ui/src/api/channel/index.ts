import { request } from '@/utils/request';
import Api from '@/core/permission/modules/channel';

export function getChannelList(params: API.PageParams<API.TChannel>) {
  return request<API.TableListResult<API.TChannelList>>({
    url: Api.list,
    method: 'get',
    params,
  });
}

export function updateChannel(data: API.TChannel) {
  return request(
    {
      url: Api.update,
      method: 'post',
      data,
    },
    {
      successMsg: '修改通道成功',
    },
  );
}

export function deleteChannel(data: { DeviceId: string; ChannelIds: string[] }) {
  return request({
    url: Api.delete,
    method: 'post',
    data,
  });
}
