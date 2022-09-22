import type { BaseResponse } from '@/utils/request';
import { request } from '@/utils/request';

export function updateAccountInfo(data: any) {
  return request<BaseResponse<any>>({
    url: 'account/update',
    method: 'post',
    data,
  });
}

export function updatePassword(data: any) {
  return request({
    url: 'account/password',
    method: 'post',
    data,
  });
}

export function getInfo() {
  return request<API.AdminUserInfo>({
    url: 'Admin/Info',
    method: 'get',
  });
}

export function permmenu() {
  return request<API.PermMenu>({
    url: 'Admin/Permmenu',
    method: 'get',
  });
}

export function logout() {
  return request({
    url: 'Admin/Logout',
    method: 'post',
  });
}
