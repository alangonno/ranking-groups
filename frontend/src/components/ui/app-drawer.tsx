import { Drawer, type DrawerProps } from "flowbite-react";

export type AppDrawerProps = DrawerProps;

export function AppDrawer(props: AppDrawerProps) {
  return <Drawer {...props} />;
}
