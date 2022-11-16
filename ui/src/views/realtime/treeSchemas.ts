import type { TreeDataItem as ATreeDataItem } from 'ant-design-vue/es/tree/Tree';

export interface TreeDataItem extends ATreeDataItem {
  tdType: number;
  parentTreeKey?: String;
  NickName: string;
  DeviceId: string;
  autoChange: boolean;
}
interface GetNickNamePar {
  NickName: string;
  DeviceId: string;
}

export const formatDevice = (devs: API.TableListResult<API.TDeviceInfoList>): TreeDataItem[] => {
  return devs.list.map((item) => {
    const name = getNickName(item);
    return Object.assign(item, {
      title: name,
      key: item.DeviceId,
      tdType: 0,
      autoChange: false,
    });
  });
};
export const getNickName = (item: GetNickNamePar): string => {
  return item.NickName && item.NickName.length
    ? `${item.NickName}(${item.DeviceId})`
    : item.DeviceId;
};

export const formatChannel = (
  devs: API.TableListResult<API.TChannelList>,
  parentKey: string,
): TreeDataItem[] => {
  return devs.list.map((item) => {
    const name =
      item.NickName && item.NickName.length
        ? `${item.NickName}(${item.ChannelId})`
        : item.ChannelId;
    return Object.assign(item, {
      title: name,
      key: `${item.DeviceId}-${item.ChannelId}`,
      isLeaf: !(item.Parental && item.ChannelId != item.DeviceId),
      tdType: 1,
      parentTreeKey: parentKey,
      autoChange: false,
    });
  });
};
// export interface TreeDataItem extends ATreeDataItem {
//   children: any;
// }
