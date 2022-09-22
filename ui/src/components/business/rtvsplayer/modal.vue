<template>
  <DraggableModal
    v-model:visible="state.visible"
    title="视频播放"
    :width="
      computed(() => {
        return props.videoWidth + 50;
      })
    "
    :force-render="true"
    :after-close="closeVideo"
  >
    <template #footer> </template>
    <RtvsPlayer
      v-bind="omit(props, ['videoWidth', 'videoHeight'])"
      ref="rtvsplayer"
      :video-nums="1"
    ></RtvsPlayer>
  </DraggableModal>
</template>

<script lang="tsx" setup>
  import { nextTick, ref, onMounted, defineExpose, computed, reactive } from 'vue';
  import { omit } from 'lodash-es';
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
  });
  const state = reactive({
    visible: false,
  });

  const closeVideo = () => {
    rtvsplayer.value.ucDo((uc) => {
      uc.Stop(0);
    });
  };
  function ucDo(ucdo: Function) {
    rtvsplayer.value.ucDo(ucdo);
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
  });
</script>
