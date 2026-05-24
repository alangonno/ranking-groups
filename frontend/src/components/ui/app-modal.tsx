import { Modal, type ModalProps } from "flowbite-react";

export type AppModalProps = ModalProps;

export function AppModal(props: AppModalProps) {
  return <Modal {...props} />;
}
