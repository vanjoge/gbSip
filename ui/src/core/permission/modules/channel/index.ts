export const channel = {
  list: 'Channel/GetChannelList',
  update: 'Channel/UpdateChannel',
  delete: 'Channel/DeleteChannel',
} as const;
export const values = Object.values(channel);

export type ChannelPerms = typeof values[number];

export default channel;
