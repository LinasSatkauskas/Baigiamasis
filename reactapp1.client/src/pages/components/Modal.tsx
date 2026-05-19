import { XMarkIcon } from "@heroicons/react/24/solid"
import { ReactNode } from "react"

type ModalProps = {
  visibleModal: boolean
  title: string
  children: ReactNode
  setVisibleModal: (s: boolean) => void
}

export function Modal(props: ModalProps) {
  const { visibleModal, title, children, setVisibleModal } = props

  return (
    <>
      {visibleModal ? (
        <>
          <div className="fixed inset-0 z-50 flex items-center justify-center overflow-x-hidden overflow-y-auto outline-none focus:outline-none">
            <div className="relative w-auto my-6 mx-auto max-w-3xl">
              {/*content*/}
              <div className="border-0 rounded-lg shadow-lg relative flex flex-col w-full bg-white outline-none focus:outline-none">
                {/*header*/}
                <div className="flex items-start justify-between p-5 border-b border-solid border-blueGray-200 rounded-t">
                  <h3 className="text-xl font-semibold">{title}</h3>
                  <button type="button" onClick={() => setVisibleModal(false)}>
                    <XMarkIcon className="h-5 w-5 stroke-gray-400" />
                  </button>
                </div>

                {/*body*/}
                <div className="relative p-6 flex-auto">
                  <div className="my-4 text-blueGray-500 text-lg leading-relaxed">
                    {children}
                  </div>
                </div>
              </div>
            </div>
          </div>
          <div className="opacity-25 fixed inset-0 z-40 bg-black"></div>
        </>
      ) : null}
    </>
  )
}
