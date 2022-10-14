<template>
  <SplitPanel>
    <template #left-content>
      <div style="height: 100%">
        <div class="flex justify-between">
          <div>{{ $t('gbs.devices') }}</div>
          <Space>
            <Tooltip placement="top">
              <template #title>{{ $t('common.redo') }}</template>
              <SyncOutlined :spin="devTreeLoading" @click="fetchDeviceList" />
            </Tooltip>
          </Space>
        </div>
        <div ref="tdiv" class="videotree">
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
            :height="tdiv?.clientHeight"
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
        </div>
        <div class="ptz">
          <div>
            <div
              class="ptz-btn ptz-up"
              @mousedown="sendPtz({ DeviceId: '', Channel: '', Up: ptzSpeed })"
              ><UpCircleTwoTone
            /></div>
            <div
              class="ptz-btn ptz-left"
              @mousedown="sendPtz({ DeviceId: '', Channel: '', Left: ptzSpeed })"
              ><LeftCircleTwoTone
            /></div>
            <div class="ptz-btn ptz-center" @click="startSpeek">
              <template v-if="state.speeking">
                <AudioTwoTone two-tone-color="#ff4d4f" />
              </template>
              <template v-else> <AudioTwoTone /> </template>
            </div>
            <div
              class="ptz-btn ptz-right"
              @mousedown="sendPtz({ DeviceId: '', Channel: '', Right: ptzSpeed })"
              ><RightCircleTwoTone
            /></div>
            <div
              class="ptz-btn ptz-down"
              @mousedown="sendPtz({ DeviceId: '', Channel: '', Down: ptzSpeed })"
              ><DownCircleTwoTone
            /></div>
            <div
              class="ptz-btn ptz-zoomin"
              @mousedown="sendPtz({ DeviceId: '', Channel: '', ZoomIn: parseInt(ptzSpeed / 17) })"
              ><PlusCircleTwoTone
            /></div>
            <div
              class="ptz-btn ptz-zoomout"
              @mousedown="sendPtz({ DeviceId: '', Channel: '', ZoomOut: parseInt(ptzSpeed / 17) })"
              ><MinusCircleTwoTone /></div
          ></div>
          <div class="ptz-speed">
            <Row align="middle">
              <Col :span="20">
                <Slider v-model:value="ptzSpeed" :min="1" :max="255" />
              </Col>
              <Col :span="2">
                <InputNumber
                  v-model:value="ptzSpeed"
                  :min="1"
                  :max="255"
                  size="small"
                  style="margin-left: 10px; width: 60px"
                />
              </Col>
            </Row>
          </div>
        </div>
      </div>
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
          @start-speek="onStartSpeek"
          @end-by-server="onEndByServer"
        ></RtvsPlayer> </div
    ></template>
  </SplitPanel>
</template>

<script setup lang="tsx">
  import { nextTick, ref, reactive, onMounted, onBeforeUnmount } from 'vue';
  //CheckCircleTwoTone,
  import {
    SyncOutlined,
    VideoCameraTwoTone,
    FolderTwoTone,
    LeftCircleTwoTone,
    RightCircleTwoTone,
    UpCircleTwoTone,
    DownCircleTwoTone,
    AudioTwoTone,
    PlusCircleTwoTone,
    MinusCircleTwoTone,
  } from '@ant-design/icons-vue';
  import { difference } from 'lodash';
  import {
    Space,
    Tree,
    Tooltip,
    Button,
    message,
    Row,
    Col,
    Slider,
    InputNumber,
  } from 'ant-design-vue';
  import { getNickName, type TreeDataItem, formatDevice, formatChannel } from './treeSchemas';
  import type { TreeProps } from 'ant-design-vue';
  import { SplitPanel } from '@/components/basic/split-panel';
  import { RtvsPlayer } from '@/components/business/rtvsplayer';
  import { getDeviceList } from '@/api/device';
  import { getChannelList } from '@/api/channel';
  import { ptzCtrl } from '@/api/deviceControl';
  import { t } from '@/hooks/useI18n';

  defineOptions({
    name: 'RealTime',
  });
  const devTreeLoading = ref(false);
  const tdiv = ref<HTMLElement>();
  const ptzSpeed = ref<number>(127);
  const sendPtz = (params: API.PPTZCtrl) => {
    const cfg = rtvsplayer.value.getUc()?.GetOperateUCVideo().config;
    if (cfg && cfg.DeviceId?.length > 0 && cfg.ChannelId?.length > 0) {
      state.nowptzDeviceId = params.DeviceId = cfg.DeviceId;
      state.nowptzChannel = params.Channel = cfg.ChannelId;
      ptzCtrl(params);
    } else {
      message.warning('请选中正在播放的窗口后再尝试');
    }
  };
  const onMouseUp = () => {
    if (state.nowptzChannel.length > 0 && state.nowptzDeviceId.length > 0) {
      //发送停止
      ptzCtrl({ DeviceId: state.nowptzDeviceId, Channel: state.nowptzChannel, Address: 0 });
      state.nowptzChannel = '';
      state.nowptzDeviceId = '';
    }
  };

  const startSpeek = () => {
    if (state.speeking) {
      rtvsplayer.value.ucDo((uc) => {
        uc.StopSpeak();
        state.speeking = false;
      });
    } else {
      const cfg = rtvsplayer.value.getUc()?.GetOperateUCVideo().config;
      if (cfg && cfg.DeviceId?.length > 0 && cfg.ChannelId?.length > 0) {
        state.speeking = true;
        rtvsplayer.value.ucDo((uc) => {
          const ret = uc.StartSpeek(cfg.DeviceId, cfg.ChannelId, {
            clusterHost: cfg.clusterHost,
            clusterPort: cfg.clusterPort,
            protocol: 2,
          });
          if (!ret) {
            alert('未能获取到麦克风，请确认当前页面是https且未设置阻止。');
          }
        });
      } else {
        message.warning('请选中正在播放的窗口后再尝试');
      }
    }
  };
  /**
   * 设备选中
   */
  const onTreeSelect = (checkedKeys: string[]) => {
    state.needDoSelect = false;
    const _add = difference(checkedKeys, state.lastCheckKeys);
    const _remove = difference(state.lastCheckKeys, checkedKeys);
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
                DeviceId: item.DeviceId,
                ChannelId: item.ChannelId,
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
        state.needDoSelect = true;
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
    state.lastCheckKeys = checkedKeys;
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

      state.lastCheckKeys = state.checkedKeys;
    }
  };
  const onStopVideo = (id: number, ucVideo) => {
    if (id > -1) {
      unCheck(state.map[ucVideo.config.treekey]);
    }
  };
  const onStartSpeek = () => {
    message.info('对讲链路建立完成，可开始对讲');
  };

  const onEndByServer = (msg: string, id: number, ucVideo) => {
    const item = state.map[ucVideo.config.treekey];
    message.error(`${t('gbs.screen')}${id} ${getNickName(item)} ${msg}`);
  };
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
        if (state.needDoSelect) {
          tmparr.push(String(item.key));
        }
      });
      if (state.needDoSelect) {
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
    speeking: boolean;
    nowptzChannel: string;
    nowptzDeviceId: string;
    needDoSelect: boolean;
    /**
     * 上次选中keys
     */
    lastCheckKeys: string[];
  }
  const state = reactive<State>({
    expandedKeys: [],
    loadedKeys: [],
    checkedKeys: [],
    treeData: [],
    map: {},
    videoWidth: 0,
    videoHeight: 0,

    speeking: false,
    nowptzChannel: '',
    nowptzDeviceId: '',
    needDoSelect: false,
    /**
     * 上次选中keys
     */
    lastCheckKeys: [],
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
    window.addEventListener('mouseup', onMouseUp);
    nextTick(() => {
      if (rcontent.value && rcontent.value.parentElement) {
        state.videoWidth = rcontent.value.parentElement.clientWidth;
        state.videoHeight = rcontent.value.parentElement.clientHeight - 50;
      }
    });
  });
  onBeforeUnmount(() => {
    window.removeEventListener('mouseup', onMouseUp);
  });
</script>

<style lang="less">
  .videotree {
    height: calc(100% - 200px);
  }

  .ptz {
    width: 250px;
    height: 200px;
    text-align: center;
    position: relative;
    font-size: 24px;
    color: #333;

    &-btn {
      width: 50px;
      height: 50px;
      line-height: 50px;
      position: absolute;
      cursor: pointer;
    }

    &-btn:hover {
      background-color: #c7e4ff;
      border-radius: 25px;
    }

    &-up {
      top: 0;
      left: 100px;
    }

    &-left {
      top: 50px;
      left: 50px;
    }

    &-center {
      top: 50px;
      left: 100px;
      border-radius: 25px;
      background-color: #effaff;
    }

    &-right {
      top: 50px;
      left: 150px;
    }

    &-down {
      top: 100px;
      left: 100px;
    }

    &-zoomin {
      top: 50px;
      left: 0px;
    }

    &-zoomout {
      top: 50px;
      left: 200px;
    }

    &-speed {
      top: 150px;
      left: 0px;
      width: 100%;
      position: absolute;
    }
  }
</style>
