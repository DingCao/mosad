#include "HelloWorldScene.h"
#include <iostream>
#include <string>

#pragma execution_character_set("utf-8")
using namespace std;

USING_NS_CC;

Scene* HelloWorld::createScene()
{
    auto scene = Scene::create();
    auto layer = HelloWorld::create();
    scene->addChild(layer);

    return scene;
}

bool HelloWorld::init()
{
    if ( !Layer::init() )
    {
        return false;
    }
    
	
    Size visibleSize = Director::getInstance()->getVisibleSize();
    Vec2 origin = Director::getInstance()->getVisibleOrigin();

	// add backgroundLayer
	Sprite* background = Sprite::create("background.jpg");
	background->setPosition(visibleSize.width / 2, visibleSize.height / 2);
	addChild(background);

	// create a fish sprite and run animation
  // add the fish sprite to the fishLayer
  fishLayer = Layer::create();
	m_fish = Sprite::createWithSpriteFrameName("fish13_01.png");
	Animate* fishAnimate = Animate::create(AnimationCache::getInstance()->getAnimation("fishAnimation"));
	m_fish->runAction(RepeatForever::create(fishAnimate));
	m_fish->setPosition(visibleSize.width / 2, visibleSize.height / 2);
	fishLayer->addChild(m_fish);

  // create a weapon sprite.
  // add the weapon sprite to the weaponLayer
  weaponLayer = Layer::create();
  m_weapon = Sprite::create("CloseSelected.png");
  m_weapon->setPosition(visibleSize.width / 2, 50);
  weaponLayer->addChild(m_weapon);

  // create a shoot button
  LabelTTF *label = LabelTTF::create("Shoot", "Arial", 20);
  MenuItemLabel *shootLabel = MenuItemLabel::create(label, CC_CALLBACK_1(HelloWorld::onShootClick, this));
  Menu *shoot = Menu::create(shootLabel, NULL);
  shoot->setPosition(visibleSize.width-40, 20);

  // add layers. first add buttom layer.
  addChild(weaponLayer);
  addChild(fishLayer);
  addChild(shoot);      // the menu must be the uppest layer.

	// add touch listener
	EventListenerTouchOneByOne* listener = EventListenerTouchOneByOne::create();
	listener->setSwallowTouches(true);
	listener->onTouchBegan = CC_CALLBACK_2(HelloWorld::onTouchBegan, this);
	Director::getInstance()->getEventDispatcher()->addEventListenerWithSceneGraphPriority(listener, this);

    return true;
}

bool HelloWorld::onTouchBegan(Touch *touch, Event *unused_event)
{
  // realization of the fish movement coded by HuangJunjie@SYSU(SNO:13331087).
  auto touchLocation = touch->getLocation();
  m_fish->runAction(MoveTo::create(1, Vec2(touchLocation.x, touchLocation.y)));  // add action to move to the point touched

  // determine if we need to change the head of the fish
  float isRotate = touchLocation.x - m_fish->getPositionX();
  if (true) {
    if (isRotate < 0)
      isRotate = 0;
    else
      isRotate = 180;
  }

  m_fish->runAction(RotateTo::create(0, isRotate));
  // realization end
	return true;
}

void HelloWorld::onShootClick(cocos2d::Ref* pSender) {
  float x = m_fish->getPositionX(), y = m_fish->getPositionY();
  m_weapon->runAction(MoveTo::create(1, Vec2(x, y)));
}
