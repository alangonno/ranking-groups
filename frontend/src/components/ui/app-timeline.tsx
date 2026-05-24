import { Timeline, type TimelineProps } from "flowbite-react";

export type AppTimelineProps = TimelineProps;

export function AppTimeline(props: AppTimelineProps) {
  return <Timeline {...props} />;
}
