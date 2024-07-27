import type { DevicePerms } from './device';
import type { ChannelPerms } from './channel';
import type { DeviceControlPerms } from './deviceControl';
import type { SuperiorPerms } from './superior';
import type { GroupPerms } from '@/core/permission/modules/group';

export type PermissionType = ReplaceAll<
  DevicePerms | ChannelPerms | DeviceControlPerms | SuperiorPerms | GroupPerms,
  '/',
  '.'
>;
