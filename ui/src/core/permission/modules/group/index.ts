export const group = {
  getGroups: 'Group/GetGroups',
  list: 'Group/GetAllGroups',
  create: 'Group/CreateGroup',
  update: 'Group/UpdateGroup',
  delete: 'Group/DeleteGroups',
  channelList: 'Group/GetChannelList',
  bindChannels: 'Group/BindChannels',
  bindSuperior: 'Group/BindSuperior',
} as const;
export const values = Object.values(group);

export type GroupPerms = typeof values[number];

export default group;
