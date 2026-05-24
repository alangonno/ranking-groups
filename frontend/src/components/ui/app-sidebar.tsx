import { Sidebar, type SidebarProps } from "flowbite-react";

export type AppSidebarProps = SidebarProps;

export function AppSidebar(props: AppSidebarProps) {
  return <Sidebar {...props} />;
}
