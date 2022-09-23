import type { DevicePerms } from './device';
import type { ChannelPerms } from './channel';
import type { DeviceControlPerms } from './deviceControl';

export type PermissionType = ReplaceAll<DevicePerms | ChannelPerms | DeviceControlPerms, '/', '.'>;
