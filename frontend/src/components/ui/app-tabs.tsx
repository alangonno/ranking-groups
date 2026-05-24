import { Tabs, type TabsProps } from "flowbite-react";

export type AppTabsProps = TabsProps;

export function AppTabs(props: AppTabsProps) {
  return <Tabs {...props} />;
}
