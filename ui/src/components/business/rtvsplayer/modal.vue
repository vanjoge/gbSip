<template>
  <DraggableModal
    v-model:visible="state.visible"
    title="视频播放"
    :width="props.videoWidth + 50"
    :force-render="true"
    :after-close="closeVideo"
  >
    <template #footer> </template>
    <RtvsPlayer
      v-bind="omit(props, ['videoWidth', 'videoHeight'])"
      ref="rtvsplayer"
      :video-nums="1"
    ></RtvsPlayer>
    <template v-if="props.showHistorySelect"
      ><div :style="{ width: props.videoWidth + 'px' }">
        <Tabs active-key="1" size="small">
          <Tabs.TabPane key="1" tab="历史视频列表">
            <Space direction="vertical" :size="12">
              <Row>
                <Col :span="20"
                  ><DatePicker.RangePicker
                    v-model:value="selectDate"
                    :show-time="{ format: 'HH:mm:ss' }"
                    format="YYYY-MM-DD
              HH:mm:ss"
                /></Col>
                <Col :span="4"
                  ><Button type="primary" :disabled="state.tableLoading" @click="queryVideoFileList"
                    >查询</Button
                  ></Col
                >
              </Row>
            </Space>
            <Table
              :columns="columns"
              :data-source="state.data"
              size="small"
              :pagination="false"
              :scroll="{ y: 150 }"
              :loading="state.tableLoading"
            >
              <template #bodyCell="{ column, record }">
                <template v-if="column.key === 'operation'">
                  <Button type="link" @click="playBack(record)">播放</Button>
                  <Button type="link" @click="downLoad(record)">下载</Button>
                </template>
              </template></Table
            >
          </Tabs.TabPane>
        </Tabs></div
      >
    </template>
  </DraggableModal>
</template>

<script lang="tsx" setup>
  import { nextTick, ref, onMounted, defineExpose, reactive } from 'vue';
  import { omit } from 'lodash-es';
  import dayjs, { Dayjs } from 'dayjs';
  import { Table, Tabs, Space, DatePicker, Row, Col } from 'ant-design-vue';
  import Button from '../../basic/button/button.vue';
  import { default as RtvsPlayer } from './index.vue';
  import { DraggableModal } from '@/components/core/draggable-modal';
  const rtvsplayer = ref(RtvsPlayer);
  const props = defineProps({
    videoWidth: {
      type: Number,
      default: 854,
    },
    videoHeight: {
      type: Number,
      default: 480,
    },
    showHistorySelect: {
      type: Boolean,
      default: false,
    },
  });
  const selectDate = ref<[Dayjs, Dayjs]>([
    dayjs(new Date(new Date().setHours(0, 0, 0, 0))),
    dayjs(new Date(new Date().setHours(0, 0, 0, 0) + 24 * 60 * 60 * 1000 - 1)),
  ]);
  const state = reactive({
    visible: false,
    sim: '',
    channel: '',
    server: '',
    port: 17000,
    data: [],
    tableLoading: false,
  });

  const columns = [
    { title: '开始时间', dataIndex: 'StartTime' },
    { title: '结束时间', dataIndex: 'EndTime' },
    { title: '文件大小(B)', dataIndex: 'FileSize' },
    { title: '', key: 'operation' },
  ];

  const time1078ToTime = (time1078) => {
    return `20${time1078.substring(0, 2)}-${time1078.substring(2, 4)}-${time1078.substring(
      4,
      6,
    )} ${time1078.substring(6, 8)}:${time1078.substring(8, 10)}:${time1078.substring(10, 12)}`;
  };
  const queryVideoFileList = () => {
    state.tableLoading = true;
    ucDo((uc) => {
      uc.QueryVideoFileList(
        state.sim,
        state.channel,
        selectDate.value[0].unix(),
        selectDate.value[1].unix(),
        '0',
        0,
        0,
        0,
        (data) => {
          state.data = [];
          for (let index = 0; index < data.FileCount; index++) {
            const element = data.FileList[index];
            state.data.push({
              StartTime: time1078ToTime(element.StartTime),
              EndTime: time1078ToTime(element.EndTime),
              FileSize: element.FileSize,
              odata: element,
            });
          }
          state.tableLoading = false;
        },
        0,
        {
          clusterHost: state.server,
          clusterPort: state.port,
          protocol: 2,
        },
        0,
      );
    });
  };

  const playBack = (record) => {
    ucDo((uc) => {
      uc.PlaybackVideo(
        state.sim,
        state.channel,
        record.odata.MediaType,
        record.odata.StreamType,
        record.odata.StorageType,
        0,
        1,
        record.StartTime,
        record.EndTime,
        0,
        {
          clusterHost: state.server,
          clusterPort: state.port,
          protocol: 2,
        },
        null,
        0,
        0,
      );
    });
  };

  const downLoad = (record) => {
    ucDo((uc) => {
      uc.DownLoadFmp4(
        state.sim,
        state.channel,
        record.odata.MediaType,
        record.odata.StreamType,
        record.odata.StorageType,
        0,
        1,
        record.StartTime,
        record.EndTime,
        {
          clusterHost: state.server,
          clusterPort: state.port,
          protocol: 2,
        },
        0,
      );
    });
  };
  const closeVideo = () => {
    ucDo((uc) => {
      uc.Stop(0);
    });
  };
  function ucDo(ucdo: Function) {
    rtvsplayer.value.ucDo(ucdo);
  }
  function setBackQuery(sim: string, channel: string, server: string, port: number) {
    state.sim = sim;
    state.channel = channel;
    state.server = server;
    state.port = port;
    ucDo((uc) => {
      uc.Resize(props.videoWidth, props.videoHeight);
    });
  }
  onMounted(() => {
    nextTick(() => {
      //
      ucDo((uc) => {
        uc.Resize(props.videoWidth, props.videoHeight);
      });
    });
  });
  defineExpose({
    ucDo,
    setBackQuery,
  });
</script>
