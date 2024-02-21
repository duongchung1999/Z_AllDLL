#ifndef _USB2883_DEVICE_
#define _USB2883_DEVICE_

#include<windows.h>

#define USB2883_MAX_AI_CHANNELS	12 // �������֧��12·ģ������������ͨ��

//***********************************************************
// ����AD�ɼ��Ĳ����ṹ	
typedef struct _USB2883_PARA_AD
{
	LONG bChannelArray[USB2883_MAX_AI_CHANNELS];	// ����ͨ��ѡ�����У��ֱ����6��ͨ����=TRUE��ʾ��ͨ�����������򲻲���	
	LONG InputRange[USB2883_MAX_AI_CHANNELS];		// ģ������������ѡ��(ǰ����ͨ�����̱���һ�£�������ͨ�����̱���һ��)
	LONG Gains[USB2883_MAX_AI_CHANNELS];			// �������
	LONG Frequency;         // �ɼ�Ƶ��,��λΪHz, [1000, 250000]
	LONG TriggerMode;		// ����ģʽѡ��
	LONG TriggerSource;		// ����Դѡ��
	LONG TriggerDir;		// ��������ѡ��(����/���򴥷�)
	LONG TrigLevelVolt;		// ������ƽ(���̰�ģ����������)
	LONG TrigWindow;		// ��������������uS,[50,1638]
	LONG ClockSource;		// ʱ��Դѡ��(��/��ʱ��Դ)
	LONG bClockOutput;      // ����ʱ�������CLKOUT,=TRUE:����ʱ�����, =FALSE:��ֹʱ�����
} USB2883_PARA_AD, *PUSB2883_PARA_AD;

//***********************************************************
// AD����USB2883_PARA_AD�е�Gains[x]ʹ�õ�Ӳ������ѡ��
const long USB2883_GAINS_1MULT			= 0x00; // 1������
const long USB2883_GAINS_2MULT			= 0x01; // 2������
const long USB2883_GAINS_4MULT			= 0x02; // 4������
const long USB2883_GAINS_8MULT			= 0x03; // 8������

//***********************************************************
// ADӲ������USB2883_PARA_AD�е�InputRange������ʹ�õ�ѡ��
const long USB2883_INPUT_N10000_P10000mV= 0x00; // ��10000mV
const long USB2883_INPUT_N5000_P5000mV	= 0x01; // ��5000mV

//***********************************************************
// ADӲ������USB2883_PARA_AD�е�TriggerMode��Ա������ʹ�ô���ģʽѡ��
const long USB2883_TRIGMODE_EDGE		= 0x00; // ���ش���
const long USB2883_TRIGMODE_PULSE		= 0x01; // ��ƽ����

//***********************************************************
// ADӲ������USB2883_PARA_AD�е�TriggerSource����Դ�ź���ʹ�õ�ѡ��
const long USB2883_TRIGMODE_SOFT		= 0x00; // �������
const long USB2883_TRIGSRC_DTR			= 0x01; // ѡ��DTR��Ϊ����Դ
const long USB2883_TRIGSRC_ATR			= 0x02; // ѡ��ATR��Ϊ����Դ
const long USB2883_TRIGSRC_TRIGGER		= 0x03; // Trigger�źŴ��������ڶ࿨ͬ����

//***********************************************************
// ADӲ������USB2883_PARA_AD�е�TriggerDir����������ʹ�õ�ѡ��
const long USB2883_TRIGDIR_NEGATIVE		= 0x00; // ���򴥷�(�͵�ƽ/�½��ش���)
const long USB2883_TRIGDIR_POSITIVE		= 0x01; // ���򴥷�(�ߵ�ƽ/�����ش���)
const long USB2883_TRIGDIR_POSIT_NEGAT	= 0x02; // �����򴥷�(��/�͵�ƽ������/�½��ش���)

//***********************************************************
// ADӲ������USB2883_PARA_AD�е�ClockSourceʱ��Դ��ʹ�õ�ѡ��
const long USB2883_CLOCKSRC_IN			= 0x00; // �ڲ�ʱ�Ӷ�ʱ����
const long USB2883_CLOCKSRC_OUT			= 0x01; // �ⲿʱ�Ӷ�ʱ����(ʹ��CN1�ϵ�CLKIN�ź�����)

//*************************************************************************************
// ����AD������ʵ��Ӳ������
typedef struct _USB2883_STATUS_AD     
{
	LONG bADEanble;	// AD�Ƿ��Ѿ�ʹ�ܣ�=TRUE:��ʾ��ʹ�ܣ�=FALSE:��ʾδʹ��
	LONG bTrigger;  // AD�Ƿ񱻴�����=TRUE:��ʾ�ѱ�������=FALSE:��ʾδ������
	LONG bHalf;		// �ɼ������Ƿ��Ѵ������=TRUE:��ʾ�Ѱ�����=FALSE:��ʾδ����
} USB2883_STATUS_AD, *PUSB2883_STATUS_AD;

//***********************************************************
// ���������ӿ�
#ifndef _USB2883_DRIVER_
#define DEVAPI __declspec(dllimport)
#else
#define DEVAPI __declspec(dllexport)
#endif

#ifdef __cplusplus
extern "C" {
#endif
	//######################## ����ͨ�ú��� #################################
	HANDLE DEVAPI FAR PASCAL USB2883_CreateDevice(int DevicePhysID = 0); // �����豸����(�ú���ʹ��ϵͳ���߼��豸ID��
	HANDLE DEVAPI FAR PASCAL USB2883_CreateDeviceEx(int DevicePhysID); // ʹ������ID�����豸����
	BOOL DEVAPI FAR PASCAL USB2883_GetDeviceCurrentID(HANDLE hDevice, PLONG DevicePhysID); // ȡ�õ�ǰ�豸������ID��
	BOOL DEVAPI FAR PASCAL USB2883_SetDevicePhysID(HANDLE hDevice, LONG DevicePhysID); // ���õ�ǰ�豸������ID��,����ID[0~255],�������ϵ�
	BOOL DEVAPI FAR PASCAL USB2883_ResetDevice(HANDLE hDevice);		 // ��λ����USB�豸
    BOOL DEVAPI FAR PASCAL USB2883_ReleaseDevice(HANDLE hDevice);    // �豸���

	//####################### AD���ݶ�ȡ���� #################################
	BOOL DEVAPI FAR PASCAL USB2883_ADCalibration(				// AD�Զ�У׼����
									HANDLE hDevice);			// �豸������,����CreateDevice��������
	
    BOOL DEVAPI FAR PASCAL USB2883_InitDeviceAD(				// ��ʼ���豸,������TRUE��,�豸���̿�ʼ����.
									HANDLE hDevice,				// �豸���,��Ӧ��CreateDevice��������
									PUSB2883_PARA_AD pADPara);  // Ӳ������, �����ڴ˺����о���Ӳ��״̬	

	BOOL DEVAPI FAR PASCAL USB2883_StartDeviceAD(				// �ڳ�ʼ��֮�������豸
									HANDLE hDevice);			// �豸������

	BOOL DEVAPI FAR PASCAL USB2883_StopDeviceAD(				// �������豸֮����ͣ�豸
									HANDLE hDevice);			// �豸������

    BOOL DEVAPI FAR PASCAL USB2883_ReadDeviceAD(				// ��ʼ���豸�󣬼����ô˺�����ȡ�豸�ϵ�AD����
									HANDLE hDevice,				// �豸���,��Ӧ��CreateDevice��������
									USHORT ADBuffer[],			// �����ڽ������ݵ��û�������
									LONG nReadSizeWords,		// ��ȡAD���ݵĳ���(��)
									PLONG nRetSizeWords,		// ʵ�ʷ������ݵĳ���(��),
									BOOL bEnoughRtn = TRUE,		// TRUE��������ȡ��������0��FALSE:������ȡ��������ʵ�ʵ���
									PLONG nSurplusWords = NULL);// ����FIFOʣ�����


    BOOL DEVAPI FAR PASCAL USB2883_ReleaseDeviceAD( HANDLE hDevice); // ֹͣAD�ɼ����ͷ�AD������ռ��Դ

   	//################# AD��Ӳ�������������� ########################	
	BOOL DEVAPI FAR PASCAL USB2883_SaveParaAD(HANDLE hDevice, PUSB2883_PARA_AD pADPara);  
    BOOL DEVAPI FAR PASCAL USB2883_LoadParaAD(HANDLE hDevice, PUSB2883_PARA_AD pADPara);
    BOOL DEVAPI FAR PASCAL USB2883_ResetParaAD(HANDLE hDevice, PUSB2883_PARA_AD pADPara); // ��AD���������ָ�������Ĭ��ֵ

	//####################### ����I/O����������� #################################
	BOOL DEVAPI FAR PASCAL USB2883_GetDeviceDI(				// ȡ�ÿ�����״̬     
									HANDLE hDevice,				// �豸���,��Ӧ��CreateDevice��������								        
									BYTE bDISts[16]);			// ��������״̬(ע��: ���붨��Ϊ16���ֽ�Ԫ�ص�����)

	BOOL DEVAPI FAR PASCAL USB2883_SetDeviceDO(				// ���������״̬
									HANDLE hDevice,				// �豸���,��Ӧ��CreateDevice��������								        
									BYTE bDOSts[16]);			// �������״̬(ע��: ���붨��Ϊ16���ֽ�Ԫ�ص�����)

	//############################################################################
	BOOL DEVAPI FAR PASCAL USB2883_GetDevVersion(				// ��ȡ�豸�̼�������汾
									HANDLE hDevice,				// �豸������,����CreateDevice��������
									PULONG pulFmwVersion,		// �̼��汾
									PULONG pulDriverVersion);	// �����汾


	//############################ �̲߳������� ################################
	HANDLE DEVAPI FAR PASCAL USB2883_CreateSystemEvent(void); 	// �����ں�ϵͳ�¼�����
	BOOL DEVAPI FAR PASCAL USB2883_ReleaseSystemEvent(HANDLE hEvent); // �ͷ��ں��¼�����

#ifdef __cplusplus
}
#endif

// �Զ������������������
#ifndef _USB2883_DRIVER_
#ifndef _WIN64
#pragma comment(lib, "USB2883_32.lib")
#pragma message("======== Welcome to use our art company's products!")
#pragma message("======== Automatically linking with USB2883_32.dll...")
#pragma message("======== Successfully linked with USB2883_32.dll")
#else
#pragma comment(lib, "USB2883_64.lib")
#pragma message("======== Welcome to use our art company's products!")
#pragma message("======== Automatically linking with USB2883_64.dll...")
#pragma message("======== Successfully linked with USB2883_64.dll")
#endif

#endif

#endif; // _USB2883_DEVICE_