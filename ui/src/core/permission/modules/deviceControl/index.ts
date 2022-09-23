export const device = {
  ptzctrl: 'DeviceControl/PTZCtrl',
} as const;
export const values = Object.values(device);

export type DeviceControlPerms = typeof values[number];

export default device;
