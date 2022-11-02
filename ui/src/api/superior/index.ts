import { request } from '@/utils/request';
import Api from '@/core/permission/modules/superior';

export function getSuperiorList(params: API.PageParams<API.TSuperior>) {
  return request<API.TableListResult<API.TSuperiorList>>({
    url: Api.list,
    method: 'get',
    params,
  });
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
