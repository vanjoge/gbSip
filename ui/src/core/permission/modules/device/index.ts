export const device = {
  list: 'DeviceInfo/GetDeviceList',
  info: 'DeviceInfo/GetDeviceInfo',
  update: 'DeviceInfo/UpdateDevice',
  delete: 'DeviceInfo/DeleteDevice',
  refreshChannel: 'DeviceInfo/SendRefreshChannel',
} as const;
export const values = Object.values(device);

export type DevicePerms = typeof values[number];

export default device;
