<template>
  <SplitPanel>
    <template #left-content>
      <div class="flex justify-between">
        <div>{{ $t('gbs.devices') }}</div>
        <Space>
          <Tooltip placement="top">
            <template #title>{{ $t('common.redo') }}</template>
            <SyncOutlined :spin="devTreeLoading" @click="fetchDeviceList" />
          </Tooltip>
        </Space>
      </div>
      <Tree
        v-model:expandedKeys="state.expandedKeys"
        v-model:checkedKeys="state.checkedKeys"
        checkable
        block-node
        show-icon
        :selectable="false"
        :tree-data="state.treeData"
        :load-data="onLoadData"
        :loaded-keys="state.loadedKeys"
        @check="onTreeSelect"
        @load="onLoad"
      >
        <template #icon="{ Online, tdType }">
          <template v-if="tdType == 0">
            <template v-if="Online">
              <FolderTwoTone two-tone-color="#52c41a" />
            </template>
            <template v-else> <FolderTwoTone two-tone-color="#a9a9a9" /></template>
          </template>
          <template v-else>
            <template v-if="Online">
              <VideoCameraTwoTone two-tone-color="#52c41a" />
            </template>
            <template v-else> <VideoCameraTwoTone two-tone-color="#a9a9a9" /></template>
          </template>
        </template>
        <!-- <template #title="{ key, title, formData }">
          <Dropdown :trigger="['contextmenu']">
            <span>{{ title }}</span>
            <template #overlay>
              <Menu>
                <Menu.Item
                  key="1"
                  :disabled="!$auth('sys.dept.update')"
                  @click="openDeptModal(formData)"
                >
                  编辑 <EditOutlined />
                </Menu.Item>
                <Menu.Item key="2" :disabled="!$auth('sys.dept.delete')" @click="delDept(key)">
                  删除 <DeleteOutlined />
                </Menu.Item>
              </Menu>
            </template>
          </Dropdown>
        </template> -->
      </Tree>
    </template>
    <template #right-content>
      <div ref="rcontent">
        <div class="flex justify-between">
          <Space>
            <Tooltip placement="top">
              <template #title>1 {{ $t('gbs.screen') }} </template>
              <Button @click="changeScreens(1)">1</Button>
            </Tooltip>
            <Tooltip placement="top">
              <template #title>2 {{ $t('gbs.screen') }} </template>
              <Button @click="changeScreens(2)">2</Button>
            </Tooltip>
            <Tooltip placement="top">
              <template #title>4 {{ $t('gbs.screen') }} </template>
              <Button @click="changeScreens(4)">4</Button>
            </Tooltip>
            <Tooltip placement="top">
              <template #title>9 {{ $t('gbs.screen') }} </template>
              <Button @click="changeScreens(9)">9</Button>
            </Tooltip>
            <Tooltip placement="top">
              <template #title>10 {{ $t('gbs.screen') }} </template>
              <Button @click="changeScreens(10)">10</Button>
            </Tooltip>
            <Tooltip placement="top">
              <template #title>16 {{ $t('gbs.screen') }} </template>
              <Button @click="changeScreens(16)">16</Button>
            </Tooltip>
          </Space>

          <Space>
            <Tooltip placement="top">
              <template #title>{{ $t('gbs.allFull') }}</template>
              <Button @click="fullScreen">{{ $t('gbs.fullScreen') }}</Button>
            </Tooltip>
          </Space>
        </div>
        <RtvsPlayer
          ref="rtvsplayer"
          :video-width="state.videoWidth.valueOf()"
          :video-height="state.videoHeight.valueOf()"
          :video-nums="10"
          @stop="onStopVideo"
          @end-by-server="onEndByServer"
        ></RtvsPlayer> </div
    ></template>
  </SplitPanel>
</template>

<script setup lang="tsx">
  import { nextTick, ref, reactive, onMounted } from 'vue';
  //CheckCircleTwoTone,
  import { SyncOutlined, VideoCameraTwoTone, FolderTwoTone } from '@ant-design/icons-vue';
  import { difference } from 'lodash';
  import { Space, Tree, Tooltip, Button, message } from 'ant-design-vue';
  import { getNickName, type TreeDataItem, formatDevice, formatChannel } from './treeSchemas';
  import type { TreeProps } from 'ant-design-vue';
  import { SplitPanel } from '@/components/basic/split-panel';
  import { RtvsPlayer } from '@/components/business/rtvsplayer';
  import { getDeviceList } from '@/api/device';
  import { getChannelList } from '@/api/channel';
  import { t } from '@/hooks/useI18n';

  defineOptions({
    name: 'RealTime',
  });
  const devTreeLoading = ref(false);
  let needDoSelect = false;
  /**
   * 设备选中
   */
  const onTreeSelect = (checkedKeys: string[]) => {
    needDoSelect = false;
    const _add = difference(checkedKeys, lastCheckKeys);
    const _remove = difference(lastCheckKeys, checkedKeys);
    _add.forEach((key) => {
      const item = state.map[key];
      if (item?.isLeaf) {
        rtvsplayer.value.ucDo((uc) => {
          const vid = rtvsplayer.value.getIdleVideoid();
          if (vid) {
            item.vid = vid;
            uc.StartRealTimeVideo(
              item.DeviceId,
              item.ChannelId,
              1,
              true,
              vid,
              {
                clusterHost: item.RTVSVideoServer,
                clusterPort: item.RTVSVideoPort,
                protocol: 2,
                treekey: key,
              },
              null,
              item.PlayerMode,
            );
          } else {
            message.error(t('gbs.notEnoughScreen'));
            unCheck(item);
          }
        });
      } else {
        state.expandedKeys.push(key);
        needDoSelect = true;
      }
    });
    _remove.forEach((key) => {
      const item = state.map[key];
      if (item?.isLeaf) {
        rtvsplayer.value.ucDo((uc) => {
          uc.Stop(item.vid);
        });
      }
    });
    lastCheckKeys = checkedKeys;
  };
  const onLoad = () => {
    // if (needDoSelect) {
    //   setTimeout(() => {
    //     onTreeSelect(state.checkedKeys);
    //   }, 500);
    // }
  };
  const unCheck = (item: TreeDataItem) => {
    if (item) {
      let predicate: (p: string) => boolean;
      if (item.parentTreeKey) {
        predicate = (p) => p !== item.key && p !== item.parentTreeKey;
      } else {
        predicate = (p) => p !== item.key;
      }
      state.checkedKeys = state.checkedKeys.filter(predicate);

      lastCheckKeys = state.checkedKeys;
    }
  };
  const onStopVideo = (_id: number, ucVideo) => {
    unCheck(state.map[ucVideo.config.treekey]);
  };

  const onEndByServer = (msg: string, id: number, ucVideo) => {
    const item = state.map[ucVideo.config.treekey];
    message.error(`${t('gbs.screen')}${id} ${getNickName(item)} ${msg}`);
  };
  /**
   * 上次选中keys
   */
  let lastCheckKeys: string[] = [];
  const rcontent = ref<HTMLDivElement>();

  const onLoadData: TreeProps['loadData'] = async (treeNode) => {
    if (treeNode.dataRef) {
      if (treeNode.dataRef.children) {
        return;
      }
      const lst = await getChannelList({
        page: 1,
        limit: -1,
        DeviceId: treeNode.DeviceId,
        ParentId: treeNode.tdType == 1 ? treeNode.ChannelId : undefined,
      }).finally(() => (devTreeLoading.value = false));
      const child = formatChannel(lst, String(treeNode.key));
      treeNode.dataRef.children = child;
      state.expandedKeys = [...state.expandedKeys];
      state.loadedKeys.push(String(treeNode.key));
      // state.loadedKeys = [];
      state.treeData = [...state.treeData];
      const tmparr: string[] = [];
      child.forEach((item) => {
        state.map[item.key] = item;
        if (needDoSelect) {
          tmparr.push(String(item.key));
        }
      });
      if (needDoSelect) {
        state.checkedKeys = [...state.checkedKeys, ...tmparr];
        onTreeSelect(state.checkedKeys);
      }
    }
  };

  const changeScreens = (num: number) => {
    rtvsplayer.value.ucDo((uc) => {
      uc.LayoutByScreens(num);
    });
  };
  const fullScreen = () => {
    rtvsplayer.value.ucDo((uc) => {
      uc.FullScreen();
    });
  };
  interface State {
    expandedKeys: string[];
    loadedKeys: string[];
    checkedKeys: string[];
    treeData: TreeDataItem[];
    videoWidth: Number;
    videoHeight: Number;
    map: {
      [idx: string]: TreeDataItem;
    };
  }
  const state = reactive<State>({
    expandedKeys: [],
    loadedKeys: [],
    checkedKeys: [],
    treeData: [],
    map: {},
    videoWidth: 0,
    videoHeight: 0,
  });

  const rtvsplayer = ref(RtvsPlayer);

  /**
   * 获取设备列表
   */
  const fetchDeviceList = async () => {
    devTreeLoading.value = true;
    state.expandedKeys = [];
    state.loadedKeys = [];
    state.checkedKeys = [];
    const lst = await getDeviceList({
      page: 1,
      limit: -1,
    }).finally(() => (devTreeLoading.value = false));
    state.map = {};
    state.treeData = formatDevice(lst);
    state.treeData.forEach((item) => {
      state.map[item.key] = item;
    });
    // state.expandedKeys = [...state.expandedKeys, ...state.treeData.map((n) => String(n.key))];
  };
  fetchDeviceList();
  onMounted(() => {
    nextTick(() => {
      if (rcontent.value && rcontent.value.parentElement) {
        state.videoWidth = rcontent.value.parentElement.clientWidth;
        state.videoHeight = rcontent.value.parentElement.clientHeight - 50;
      }
    });
  });
</script>
