import type { DevicePerms } from './device';
import type { ChannelPerms } from './channel';
import type { DeviceControlPerms } from './deviceControl';
import type { SuperiorPerms } from './superior';

export type PermissionType = ReplaceAll<
  DevicePerms | ChannelPerms | DeviceControlPerms | SuperiorPerms,
  '/',
  '.'
>;
