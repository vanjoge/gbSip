import { request } from '@/utils/request';
import Api from '@/core/permission/modules/device';

export function getDeviceList(params: API.PageParams<API.TDeviceInfo>) {
  return request<API.TableListResult<API.TDeviceInfoList>>({
    url: Api.list,
    method: 'get',
    params,
  });
}

export function updateDevice(data: API.TDeviceInfo) {
  return request(
    {
      url: Api.update,
      method: 'post',
      data,
    },
    {
      successMsg: '修改设备成功',
    },
  );
}

export function deleteDevice(data: { DeviceIds: string[] }) {
  return request({
    url: Api.delete,
    method: 'post',
    data,
  });
}
export function refreshChannels(params: { DeviceId: string }) {
  return request({
    url: Api.refreshChannel,
    method: 'post',
    params,
  });
}
