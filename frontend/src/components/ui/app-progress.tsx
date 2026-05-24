import { Progress, type ProgressProps } from "flowbite-react";

export type AppProgressProps = ProgressProps;

export function AppProgress(props: AppProgressProps) {
  return <Progress {...props} />;
}
