<template>
  <div ref="root" :style="{ width: videoWidth + 'px', height: videoHeight + 'px' }"></div>
</template>

<script lang="ts" setup>
  import { nextTick, ref, onMounted, watch, onBeforeUnmount } from 'vue';
  const root = ref(null);
  const props = defineProps({
    videoWidth: {
      type: Number,
      default: 854,
    },
    videoHeight: {
      type: Number,
      default: 480,
    },
    videoNums: {
      type: Number,
      default: 4,
    },
    onStop: {
      type: Function,
    },
    onChangeH265Player: {
      type: Function,
    },
    onEndByServer: {
      type: Function,
    },
    onStartSpeek: {
      type: Function,
    },
  });
  let _initOK = false;
  let uc1: any;
  const initOK = ref(_initOK);
  const waitDo: Function[] = [];
  /**
   * name
   */
  function getUc() {
    return uc1;
  }
  function ucDo(ucdo: Function) {
    if (_initOK) {
      if (uc1.GetContainerSize().height == 0) {
        // uc1.Resize(props.videoWidth, props.videoHeight);
        //未能拿到显示事件，因此延迟100ms后再做Resize
        setTimeout(() => {
          uc1.Resize(props.videoWidth, props.videoHeight);
        }, 100);
      }
      ucdo(uc1);
    } else {
      waitDo.push(ucdo);
    }
  }
  let svid = 0;
  function getIdleVideoid(autoClose: boolean) {
    for (let index = 1; index <= 16; index++) {
      if (!uc1.IsPlaying(index)) {
        svid = 0;
        return index;
      }
    }
    if (autoClose) {
      uc1.Stop(++svid);
      return svid;
    }
  }

  onMounted(() => {
    nextTick(() => {
      uc1 = CvNetVideo.Init(root.value, props.videoNums, {
        callback() {
          _initOK = true;
          if (uc1) {
            while (waitDo.length > 0) {
              const ucdo = waitDo.shift();
              if (ucdo) ucdo(uc1);
            }
          }
        },
        //事件通知
        events: {
          //停止视频时事件
          //参数1 id 表示第几个分屏 从1开始 -1表示对讲通道
          //参数2 停止的UCVideo对象
          onStop: props.onStop,
          //
          onChangeH265Player: props.onChangeH265Player,
          //服务端结束
          //参数1 结束原因 字符串
          //参数2 id 表示第几个分屏 从1开始 -1表示对讲通道
          //参数3 UCVideo对象
          //返回值表示是否取消自动停止，为真时表示取消
          onEndByServer: props.onEndByServer,
          //对讲开始 与设备链路建立完成，且可开始对讲时触发
          //参数 无
          onStartSpeek: props.onStartSpeek,
        },
      });
      if (_initOK) {
        while (waitDo.length > 0) {
          const ucdo = waitDo.shift();
          if (ucdo) ucdo(uc1);
        }
      }
    });
  });
  onBeforeUnmount(() => {
    uc1?.Stop(-1);
  });
  defineExpose({
    getUc,
    ucDo,
    initOK,
    getIdleVideoid,
  });
  watch(
    () => props,
    async () => {
      if (props) {
        // console.log('table onMounted');
        await nextTick();
        ucDo((uc) => {
          uc.Resize(props.videoWidth, props.videoHeight);
        });
      }
    },
    {
      immediate: true,
      deep: true,
    },
  );

  // watch(
  //   () => props.videoWidth,
  //   () => {
  //     uc1?.Resize(props.videoWidth, props.videoHeight);
  //   },
  // );
  // watch(
  //   () => props.videoHeight,
  //   () => {
  //     uc1?.Resize(props.videoWidth, props.videoHeight);
  //   },
  // );
</script>

//此处需要覆盖box-sizing，否则antd会造成分屏间距过大，也可演示如何自定义css覆盖rtvs控件原有样式
<style>
  .video-box-body {
    float: none;
    overflow: hidden;
    position: absolute;
    border: 2px solid #000000;
    box-sizing: content-box !important;
  }
</style>
