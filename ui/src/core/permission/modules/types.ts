import type { DevicePerms } from './device';
import type { ChannelPerms } from './channel';

export type PermissionType = ReplaceAll<DevicePerms | ChannelPerms, '/', '.'>;
