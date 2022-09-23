import { request } from '@/utils/request';
import Api from '@/core/permission/modules/deviceControl';

export function ptzCtrl(params: API.PPTZCtrl) {
  return request<boolean>({
    url: Api.ptzctrl,
    method: 'get',
    params,
  });
}
