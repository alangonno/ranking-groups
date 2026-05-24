import { Tooltip, type TooltipProps } from "flowbite-react";

export type AppTooltipProps = TooltipProps;

export function AppTooltip(props: AppTooltipProps) {
  return <Tooltip {...props} />;
}
